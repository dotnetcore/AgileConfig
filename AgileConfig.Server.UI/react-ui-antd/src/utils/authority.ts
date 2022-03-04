import { reloadAuthorized } from './Authorized';

// use localStorage to store the authority info, which might be sent from server in actual project.
export function getAuthority(str?: string): string | string[] {
  const authorityString =
    typeof str === 'undefined' && localStorage ? localStorage.getItem('antd-pro-authority') : str;
  // authorityString could be admin, "admin", ["admin"]
  let authority;
  try {
    if (authorityString) {
      authority = JSON.parse(authorityString);
    }
  } catch (e) {
    authority = authorityString;
  }
  if (typeof authority === 'string') {
    return [authority];
  }
  // preview.pro.ant.design only do not use in your production.
  // preview.pro.ant.design 专用环境变量，请不要在你的项目中使用它。
  if (!authority && ANT_DESIGN_PRO_ONLY_DO_NOT_USE_IN_YOUR_PRODUCTION === 'site') {
    return ['admin'];
  }
  return authority;
}

export function setAuthority(authority: string | string[] ): void {
  const proAuthority = typeof authority === 'string' ? [authority] : authority;
  localStorage.setItem('antd-pro-authority', JSON.stringify(proAuthority));
  // auto reload
  reloadAuthorized();
}

// use localStorage to store the authority info, which might be sent from server in actual project.
export function getFunctions(str?: string): string[] {
  const authorityString =
    typeof str === 'undefined' && localStorage ? localStorage.getItem('antd-pro-functions') : str;
  // authorityString could be admin, "admin", ["admin"]
  let authority;
  try {
    if (authorityString) {
      authority = JSON.parse(authorityString);
    }
  } catch (e) {
    authority = authorityString;
  }
  if (typeof authority === 'string') {
    return [authority];
  }

  return authority;
}

export function setFunctions(authority: string | string[] ): void {
  const proAuthority = typeof authority === 'string' ? [authority] : authority;
  localStorage.setItem('antd-pro-functions', JSON.stringify(proAuthority));
  // auto reload
  reloadAuthorized();
}

export function setToken(token:string): void {
  localStorage.setItem('token', token);
}

export function getToken(): string {
  const tk = localStorage.getItem('token');
  if (tk) {
    return tk as string;
  }

  return '';
}

export function setUserInfo(user:{name:string, userid:string}) {
  const json = JSON.stringify(user);
  localStorage.setItem('userinfo', json);
}

export function getUserInfo():{name:string, userid:string} {
  const json = localStorage.getItem('userinfo');
  if (json){
    return JSON.parse(json);
  }

  return {name:'',userid:''};
}
