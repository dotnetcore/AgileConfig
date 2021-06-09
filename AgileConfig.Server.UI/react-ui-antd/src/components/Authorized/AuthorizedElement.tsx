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
    var key = functions.find(x=>x ==='global_'+ props.judgeKey );
    if (key) return true;

    key = functions.find(x=>x ==='app_'+ props.appId + '_' + props.judgeKey );
    if (key) return true;

    return false;
  }
  return check()? <>{props.children}</> : <>{props.noMatch}</>
};

export default AuthorizedEle;
