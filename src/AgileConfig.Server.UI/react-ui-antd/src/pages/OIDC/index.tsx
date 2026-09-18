import { PageContainer } from '@ant-design/pro-components';
import React, { useEffect } from 'react';
import { Spin } from 'antd';
import styles from './index.less';
import { oidcLogin } from '@/services/login';
import type { Dispatch } from '@umijs/max';
import { connect, history, useIntl, useLocation } from '@umijs/max';
import { message } from 'antd';

type OidcCallbackProps = {
  dispatch: Dispatch;
};

const OidcCallback: React.FC<OidcCallbackProps> = ({ dispatch }) =>  {
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
        dispatch({
          type: 'login/changeLoginStatus',
          payload: response,
        });
        message.success(intl.formatMessage({ id: 'pages.login.loginsuccess' }));
        history.replace('/');
        return;
      }

      message.error(intl.formatMessage({ id: 'pages.login.loginfail' }));
      history.replace('/user/login');
    });
  }, [code, intl, dispatch]);

  return (
    <PageContainer>
      <div className={styles.loading}>
        <Spin tip="OIDC loading..." size='large'>
        </Spin>
      </div>
    </PageContainer>
  );
}
export default connect(() => ({}))(OidcCallback);
