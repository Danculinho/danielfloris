# File Upload Security Rules

This service implements secure file handling for job attachments:

- **MIME + extension whitelist**: only `application/pdf` + `.pdf`, `application/dxf` + `.dxf`.
- **Size limits**: default max 10 MB, configurable by `MAX_FILE_SIZE_BYTES`.
- **Antivirus hook**: `antivirusHook()` runs after file write. Current implementation is a feature-flagged hook for future AV integration (`ENABLE_ANTIVIRUS_HOOK=true`).
- **Server-generated file names**: files are stored with `GUID + extension`; user-provided names are metadata only.
- **Metadata storage**: database file `data/metadata.json` stores metadata and **relative storage path**.
- **Directory structure**: `uploads/<year>/<jobId>/<guid>.<ext>`.
- **Deletion on job cancellation**: `DELETE /jobs/:jobId/cancel` removes all files + metadata for the job.
- **Path traversal protection**: all filesystem paths are resolved under `uploads/`; traversal attempts are blocked.
- **Authorized download**: `GET /files/:fileId/download` enforces role (`x-role`) and ownership checks.

## API

### Upload file
`POST /jobs/:jobId/files`

Headers:
- `x-role`: `admin` | `manager` | `viewer`
- `x-user-id`: required for ownership checks

Body:
- multipart/form-data field `file`

### Download file
`GET /files/:fileId/download`

Headers:
- `x-role`
- `x-user-id`

### Cancel job and delete attachments
`DELETE /jobs/:jobId/cancel`

Headers:
- `x-role`: `admin` | `manager`
