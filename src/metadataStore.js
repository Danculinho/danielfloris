import fs from 'fs/promises';
import path from 'path';

const DB_PATH = path.join(process.cwd(), 'data', 'metadata.json');

export class MetadataStore {
  constructor() {
    this.records = new Map();
    this.ready = this.#load();
  }

  async #load() {
    await fs.mkdir(path.dirname(DB_PATH), { recursive: true });
    try {
      const raw = await fs.readFile(DB_PATH, 'utf8');
      const list = JSON.parse(raw);
      for (const item of list) {
        this.records.set(item.id, item);
      }
    } catch {
      await this.#persist();
    }
  }

  async #persist() {
    const payload = JSON.stringify([...this.records.values()], null, 2);
    await fs.writeFile(DB_PATH, payload, 'utf8');
  }

  async save(record) {
    await this.ready;
    this.records.set(record.id, record);
    await this.#persist();
    return record;
  }

  async findById(id) {
    await this.ready;
    return this.records.get(id) || null;
  }

  async findByJobId(jobId) {
    await this.ready;
    return [...this.records.values()].filter((item) => item.jobId === jobId);
  }

  async remove(id) {
    await this.ready;
    const existed = this.records.delete(id);
    await this.#persist();
    return existed;
  }

  async removeByJobId(jobId) {
    await this.ready;
    const removed = [];
    for (const record of [...this.records.values()]) {
      if (record.jobId === jobId) {
        this.records.delete(record.id);
        removed.push(record);
      }
    }
    await this.#persist();
    return removed;
  }
}
