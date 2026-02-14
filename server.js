import express from 'express';
import multer from 'multer';
import fs from 'fs/promises';
import path from 'path';
import {
  MAX_FILE_SIZE_BYTES,
  antivirusHook,
  createJobDirectory,
  createServerFilename,
  makeSafeAbsolutePath,
  relativeFromRoot,
  removeJobDirectory,
  removePhysicalFile,
  sanitizePathPart,
  validateFileSize,
  validateFileType
} from './src/storage.js';
import { MetadataStore } from './src/metadataStore.js';

const app = express();
const upload = multer({ storage: multer.memoryStorage(), limits: { fileSize: MAX_FILE_SIZE_BYTES } });
const store = new MetadataStore();

app.use(express.json());

function requireRole(allowedRoles) {
  return (req, res, next) => {
    const role = (req.header('x-role') || '').toLowerCase();
    if (!allowedRoles.includes(role)) {
      return res.status(403).json({ message: 'Forbidden by role policy.' });
    }
    req.user = {
      id: req.header('x-user-id') || null,
      role
    };
    return next();
  };
}

function canRead(record, user) {
  if (user.role === 'admin' || user.role === 'manager') {
    return true;
  }
  return user.id && user.id === record.ownerId;
}

app.post('/jobs/:jobId/files', requireRole(['admin', 'manager', 'viewer']), upload.single('file'), async (req, res) => {
  try {
    const jobId = sanitizePathPart(req.params.jobId);
    if (!req.file) {
      return res.status(400).json({ message: 'File is required.' });
    }

    const extension = validateFileType(req.file);
    validateFileSize(req.file.size);

    const { absoluteDir } = await createJobDirectory(jobId);
    const serverName = createServerFilename(extension);
    const absolutePath = path.join(absoluteDir, serverName);

    await fs.writeFile(absolutePath, req.file.buffer);
    await antivirusHook(absolutePath);

    const relativePath = relativeFromRoot(absolutePath);
    const metadata = {
      id: serverName,
      jobId,
      relativePath,
      originalName: req.file.originalname,
      mimeType: req.file.mimetype,
      sizeBytes: req.file.size,
      ownerId: req.user.id,
      createdAt: new Date().toISOString()
    };

    await store.save(metadata);
    return res.status(201).json({ file: metadata });
  } catch (error) {
    return res.status(400).json({ message: error.message });
  }
});

app.get('/files/:fileId/download', requireRole(['admin', 'manager', 'viewer']), async (req, res) => {
  try {
    const fileId = req.params.fileId;
    const record = await store.findById(fileId);
    if (!record) {
      return res.status(404).json({ message: 'File not found.' });
    }
    if (!canRead(record, req.user)) {
      return res.status(403).json({ message: 'You are not allowed to read this file.' });
    }

    const absolutePath = makeSafeAbsolutePath(record.relativePath);
    await fs.access(absolutePath);

    res.setHeader('Content-Type', record.mimeType);
    res.setHeader('Content-Disposition', `attachment; filename="${record.id}"`);
    return res.sendFile(absolutePath);
  } catch (error) {
    return res.status(400).json({ message: error.message });
  }
});

app.delete('/jobs/:jobId/cancel', requireRole(['admin', 'manager']), async (req, res) => {
  try {
    const jobId = sanitizePathPart(req.params.jobId);
    const files = await store.findByJobId(jobId);

    for (const file of files) {
      await removePhysicalFile(file.relativePath);
      await store.remove(file.id);
    }

    await removeJobDirectory(jobId);

    return res.json({
      message: 'Job cancelled. Files and metadata removed.',
      removedFiles: files.map((f) => f.id)
    });
  } catch (error) {
    return res.status(400).json({ message: error.message });
  }
});

const PORT = Number(process.env.PORT || 3000);
app.listen(PORT, () => {
  console.log(`File service listening on port ${PORT}`);
});
