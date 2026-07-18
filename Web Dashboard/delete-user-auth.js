import admin from 'firebase-admin';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const keyPath = path.join(__dirname, 'service-account.json');

if (!fs.existsSync(keyPath)) {
  console.error("Error: File service-account.json tidak ditemukan!");
  console.error("Pastikan Anda sudah mendownload file key dari Firebase Console,");
  console.error("meletakkannya di folder 'Web Dashboard', dan menamainya 'service-account.json'.");
  process.exit(1);
}

const serviceAccount = JSON.parse(fs.readFileSync(keyPath, 'utf8'));

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount)
});

const identifier = process.argv[2];
if (!identifier) {
  console.log("Cara pakai: node delete-user-auth.js <email_atau_uid>");
  process.exit(1);
}

async function deleteUser() {
  try {
    let userRecord;
    if (identifier.includes('@')) {
      userRecord = await admin.auth().getUserByEmail(identifier);
    } else {
      userRecord = await admin.auth().getUser(identifier);
    }
    
    await admin.auth().deleteUser(userRecord.uid);
    console.log(`\x1b[32m✅ Berhasil menghapus user dari Firebase Auth: ${userRecord.email} (UID: ${userRecord.uid})\x1b[0m`);
  } catch (error) {
    console.error("\x1b[31m❌ Gagal menghapus user:\x1b[0m", error.message);
  }
}

deleteUser();
