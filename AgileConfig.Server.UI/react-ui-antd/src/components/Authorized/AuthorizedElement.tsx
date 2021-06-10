import { getFunctions } from '@/utils/authority';
import React from 'react';
type AuthorizedProps = {
  appId?: string,
  authority?: string[],
  judgeKey: string,
  noMatch?: React.ReactNode;
};

export const checkUserPermission = (functions:string[],judgeKey:string, appid:string|undefined)=>{
  let appId = '';
  if (appid) {
    appId = appid ;
  }
  let matchKey = ('GLOBAL_'+ judgeKey);
  let key = functions.find(x=>x === matchKey);
  if (key) return true;

  matchKey = ('APP_'+ appId + '_' + judgeKey);
  key = functions.find(x=>x === matchKey);
  if (key) return true;

  return false;
}

const AuthorizedEle: React.FunctionComponent<AuthorizedProps>  = (props)=>{
  
  let functions:string[] = [];
  if (props.authority) {
    functions = props.authority;
  } else {
    functions = getFunctions();
  }

  return checkUserPermission(functions,props.judgeKey,props?.appId)? <>{props.children}</> : <>{props.noMatch}</>
};

export default AuthorizedEle;

