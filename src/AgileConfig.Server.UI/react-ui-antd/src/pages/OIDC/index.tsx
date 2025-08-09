import { PageContainer } from '@ant-design/pro-layout';
import React, { useEffect, useState } from 'react';
import { Spin } from 'antd';
import styles from './index.less';
import { oidcLogin } from '@/services/login';
import Model from '@/models/login';
import { history,getIntl,getLocale } from 'umi';
import { message } from 'antd';

const logs:React.FC = (props: any) =>  {
  const intl = getIntl(getLocale());
  const search = props.location.search; // Get query string from URL, e.g. "?foo=bar"
  const params = new URLSearchParams(search); // Use URLSearchParams to parse query string
  const code = params.get('code'); 
  console.log('OIDC code', code);
  if (code) {
    const loginModel =  Model;
    oidcLogin(code).then(( response )=>{
      if(response.status === 'ok') {
        const msg = intl.formatMessage({
          id: 'pages.login.loginsuccess'
        });
        message.success(msg);
        loginModel.reducers.changeLoginStatus({}, {
          payload: response,
          type: 'changeLoginStatus'
        });
        history.replace('/');
      }
      else {
        const msg = intl.formatMessage({
          id: 'pages.login.loginfail'
        });
        message.error(msg);
        loginModel.effects.logout();
      }
    });
  }
  return (
    <PageContainer>
      <div className={styles.loading}>
        <Spin tip="OIDC loading..." size='large'>
        </Spin>
      </div>
    </PageContainer>
  );
}
export default logs;
