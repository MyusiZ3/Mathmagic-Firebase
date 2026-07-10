const cp = require('child_process');
try {
  console.log('Running npm install via Node child_process with short path cwd...');
  cp.execSync('npm install firebase --no-audit --no-fund', {
    cwd: 'C:\\Users\\muham\\Documents\\__MATH~1\\Mathmagic-New\\Web Dashboard',
    stdio: 'inherit'
  });
  console.log('Successfully installed firebase!');
} catch (err) {
  console.error('Installation failed:', err.message);
  process.exit(1);
}
