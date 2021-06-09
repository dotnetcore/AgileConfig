import { getFunctions } from '@/utils/authority';
import React from 'react';
type AuthorizedProps = {
  appId?: string,
  authority?: string[],
  judgeKey: string,
  noMatch?: React.ReactNode;
};

const AuthorizedEle: React.FunctionComponent<AuthorizedProps>  = (props)=>{
  
  let functions:string[] = [];
  if (props.authority) {
    functions = props.authority;
  } else {
    functions = getFunctions();
  }
  const check = ()=>{
    let appId = '';
    if (props.appId) {
      appId = props.appId;
    }
    let matchKey = ('GLOBAL_'+ props.judgeKey);
    let key = functions.find(x=>x === matchKey);
    if (key) return true;

    matchKey = ('APP_'+ appId + '_' + props.judgeKey);
    key = functions.find(x=>x === matchKey);
    if (key) return true;

    return false;
  }
  return check()? <>{props.children}</> : <>{props.noMatch}</>
};

export default AuthorizedEle;
