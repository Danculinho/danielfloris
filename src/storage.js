import fs from 'fs/promises';
import path from 'path';
import { randomUUID } from 'crypto';

const STORAGE_ROOT = path.join(process.cwd(), 'uploads');
const MAX_FILE_SIZE_BYTES = Number(process.env.MAX_FILE_SIZE_BYTES || 10 * 1024 * 1024);

const ALLOWED_MIME = new Set([
  'application/pdf',
  'application/dxf'
]);

const ALLOWED_EXTENSIONS = new Set(['.pdf', '.dxf']);

export function validateFileType({ mimetype, originalname }) {
  const ext = path.extname(originalname || '').toLowerCase();
  const mimeAllowed = ALLOWED_MIME.has((mimetype || '').toLowerCase());
  const extAllowed = ALLOWED_EXTENSIONS.has(ext);

  if (!mimeAllowed || !extAllowed) {
    throw new Error('Unsupported file type. Only PDF and DXF are allowed.');
  }

  return ext;
}

export function validateFileSize(fileSize) {
  if (fileSize > MAX_FILE_SIZE_BYTES) {
    throw new Error(`File is too large. Maximum is ${MAX_FILE_SIZE_BYTES} bytes.`);
  }
}

export async function antivirusHook(filePath) {
  if (process.env.ENABLE_ANTIVIRUS_HOOK !== 'true') {
    return { clean: true, scanner: 'disabled' };
  }

  const simulated = process.env.ANTIVIRUS_RESULT || 'clean';
  if (simulated !== 'clean') {
    throw new Error('Antivirus scan failed. File blocked.');
  }

  return { clean: true, scanner: `hook:${path.basename(filePath)}` };
}

export async function createJobDirectory(jobId) {
  const year = new Date().getFullYear().toString();
  const safeJobId = sanitizePathPart(jobId);
  const relativeDir = path.join(year, safeJobId);
  const absoluteDir = path.join(STORAGE_ROOT, relativeDir);
  await fs.mkdir(absoluteDir, { recursive: true });
  return { year, safeJobId, relativeDir, absoluteDir };
}

export function sanitizePathPart(value) {
  const safe = String(value || '').replace(/[^a-zA-Z0-9_-]/g, '');
  if (!safe) {
    throw new Error('Invalid path segment.');
  }
  return safe;
}

export function createServerFilename(extension) {
  return `${randomUUID()}${extension}`;
}

export function makeSafeAbsolutePath(relativeFilePath) {
  const normalizedRelative = path.normalize(relativeFilePath).replace(/^([.]{2}[/\\])+/, '');
  const absolute = path.resolve(STORAGE_ROOT, normalizedRelative);
  const rootResolved = path.resolve(STORAGE_ROOT);
  if (!absolute.startsWith(rootResolved + path.sep) && absolute !== rootResolved) {
    throw new Error('Path traversal blocked.');
  }
  return absolute;
}

export function relativeFromRoot(absolutePath) {
  return path.relative(STORAGE_ROOT, absolutePath);
}

export async function removePhysicalFile(relativeFilePath) {
  const absolute = makeSafeAbsolutePath(relativeFilePath);
  await fs.rm(absolute, { force: true });
}

export async function removeJobDirectory(jobId) {
  const safeJobId = sanitizePathPart(jobId);
  const year = new Date().getFullYear().toString();
  const dir = path.join(STORAGE_ROOT, year, safeJobId);
  await fs.rm(dir, { recursive: true, force: true });
}

export { STORAGE_ROOT, MAX_FILE_SIZE_BYTES };
