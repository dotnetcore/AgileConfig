export function setSysInfo(appver:string, envList:string[]) {
    localStorage.setItem('appver', appver);
    const json = JSON.stringify(envList);
    localStorage.setItem('envList', json);
  }
  export function getAppVer():string {
    let ver = localStorage.getItem('appver');
    return ver ? ver: '';
  }
  export function getEnvList():string[] {
    let envListJson = localStorage.getItem('envList');
    if (envListJson) {
      return JSON.parse(envListJson);
    }
    return [];
  }
  