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
  // preview.pro.ant.design specific environment variable, please do not use it in your project.
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

// categories (business domains) control which left-menu entries are visible
export function getCategories(str?: string): string[] {
  console.log('getCategories called');
  const catString =
    typeof str === 'undefined' && localStorage ? localStorage.getItem('antd-pro-categories') : str;
  let cats;
  try {
    if (catString) {
      cats = JSON.parse(catString);
    }
  } catch (e) {
    cats = catString;
  }
  if (typeof cats === 'string') {
    return [cats];
  }
  return cats || [];
}

export function setCategories(categories: string | string[]): void {
  console.log('setCategories called with', categories);
  const arr = typeof categories === 'string' ? [categories] : categories;
  const safeArr = arr || [];
  localStorage.setItem('antd-pro-categories', JSON.stringify(safeArr));
  // menu re-eval
  reloadAuthorized();
}

// convenience helpers
export function hasFunction(fnKey: string): boolean {
  const fns = getFunctions();
  const has = Array.isArray(fns) ? fns.includes(fnKey) : false;

  console.log(`${fns} hasFunction(${fnKey}) => ${has}`);

  return has;
}

export function hasCategory(cat: string): boolean {
  const cats = getCategories();
  return Array.isArray(cats) ? cats.includes(cat) : false;
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
