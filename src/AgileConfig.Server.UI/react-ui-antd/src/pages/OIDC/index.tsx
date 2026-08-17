import { PageContainer } from '@ant-design/pro-components';
import React, { useEffect } from 'react';
import { Spin } from 'antd';
import styles from './index.less';
import { oidcLogin } from '@/services/login';
import { history, useIntl, useLocation } from '@umijs/max';
import { message } from 'antd';

const OidcCallback: React.FC = () =>  {
  const intl = useIntl();
  const location = useLocation();
  const code = new URLSearchParams(location.search).get('code');

  useEffect(() => {
    if (!code) {
      history.replace('/user/login');
      return;
    }

    oidcLogin(code).then((response) => {
      if (response.status === 'ok') {
        message.success(intl.formatMessage({ id: 'pages.login.loginsuccess' }));
        history.replace('/');
        return;
      }

      message.error(intl.formatMessage({ id: 'pages.login.loginfail' }));
      history.replace('/user/login');
    });
  }, [code, intl]);

  return (
    <PageContainer>
      <div className={styles.loading}>
        <Spin tip="OIDC loading..." size='large'>
        </Spin>
      </div>
    </PageContainer>
  );
}
export default OidcCallback;
