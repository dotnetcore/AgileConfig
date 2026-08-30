const { execFileSync, spawn } = require('child_process');
const { cpSync, existsSync, mkdtempSync, mkdirSync } = require('fs');
const { tmpdir } = require('os');
const path = require('path');

const frontendRoot = path.resolve(__dirname, '..', '..');
const repositoryRoot = path.resolve(frontendRoot, '..', '..', '..');
const apiProject = path.join(repositoryRoot, 'src', 'AgileConfig.Server.Apisite');
const apiOutput = path.join(apiProject, 'bin', 'Release', 'net10.0');
const uiSource = path.join(frontendRoot, 'dist');
const uiDestination = path.join(apiOutput, 'wwwroot', 'ui');
const runtimeDirectory = mkdtempSync(path.join(tmpdir(), 'agileconfig-e2e-'));
const databasePath = path.join(runtimeDirectory, 'agileconfig.db');

function run(command, args, cwd) {
  execFileSync(command, args, { cwd, stdio: 'inherit', shell: process.platform === 'win32' });
}

if (!existsSync(uiSource)) {
  run('npm', ['run', 'build'], frontendRoot);
}
run('dotnet', ['build', apiProject, '-c', 'Release', '--nologo'], repositoryRoot);

if (!existsSync(uiSource)) {
  throw new Error(`React build output was not found at ${uiSource}.`);
}

mkdirSync(path.dirname(uiDestination), { recursive: true });
cpSync(uiSource, uiDestination, { recursive: true, force: true });

const environment = {
  ...process.env,
  ASPNETCORE_ENVIRONMENT: 'Production',
  urls: 'http://127.0.0.1:5187',
  adminConsole: 'true',
  saPassword: 'e2e-admin-password',
  defaultApp: '',
  cluster: 'false',
  preview_mode: 'false',
  alwaysTrustSsl: 'false',
  serviceHealthCheckInterval: '60',
  serviceUnhealthInterval: '60',
  removeServiceInterval: '0',
  db__provider: 'sqlite',
  db__conn: `Data Source=${databasePath}`,
  otlp__logs__endpoint: '',
  otlp__traces__endpoint: '',
  otlp__metrics__endpoint: '',
  SSO__enabled: 'false',
};

const server = spawn('dotnet', ['run', '--no-build', '-c', 'Release', '--project', apiProject], {
  cwd: repositoryRoot,
  env: environment,
  stdio: 'inherit',
});

function stopServer() {
  if (!server.killed) server.kill();
}

process.once('SIGINT', stopServer);
process.once('SIGTERM', stopServer);
server.once('exit', (code) => process.exit(code ?? 0));
