import {
  LockTwoTone,
  UserOutlined,
} from '@ant-design/icons';
import React, { useEffect, useState } from 'react';
import ProForm, { ProFormText } from '@ant-design/pro-form';
import { useIntl, connect, FormattedMessage } from 'umi';
import type { Dispatch } from 'umi';
import { history } from 'umi';
import type { StateType } from '@/models/login';
import type { LoginParamsType } from '@/services/login';
import type { ConnectState } from '@/models/connect';
import styles from './index.less';
import { Button } from 'antd';

export type LoginProps = {
  dispatch: Dispatch;
  userLogin: StateType;
  submitting?: boolean;
};
import { sys } from '@/services/system';

const Login: React.FC<LoginProps> = (props) => {
  const { submitting } = props;
  const [type] = useState<string>('account');
  const [ssoEnabled, setSsoEnabled] = useState<boolean>(false);
  const [ssoLoginButtonText, setSsoLoginButtonText] = useState<string>('SSO Login');
  const intl = useIntl();

  useEffect(()=>{
    sys().then(resp=> {
      console.log(resp);
      if (!resp.passwordInited) {
        history.replace('/user/initpassword');
      }
      setSsoEnabled(resp.ssoEnabled);
      if(resp.ssoButtonText) {
        setSsoLoginButtonText(resp.ssoButtonText);
      }
    })
  },[])

  const handleSubmit = (values: LoginParamsType) => {
    const { dispatch } = props;
    dispatch({
      type: 'login/login',
      payload: { ...values, type },
    });
  };
  return (
    <div className={styles.main}>
      <ProForm
        initialValues={{
          autoLogin: true,
        }}
        submitter={{
          render: (_, dom) => dom.pop(),
          submitButtonProps: {
            loading: submitting,
            size: 'large',
            style: {
              width: '100%',
            },
          },
          searchConfig: {
            submitText: intl.formatMessage({
              id: 'pages.login.submit',
            })
          }
        }}
        onFinish={(values) => {
          handleSubmit(values as LoginParamsType);
          return Promise.resolve();
        }}
      >
        <div style={
            {
              textAlign: "center",
              color: "#1890ff",
              fontSize: "16px",
              marginBottom: "20px"
            }
        }>
          {
            intl.formatMessage({
              id: 'pages.login.accountLogin.tab',
            })
          }
        </div>
        <ProFormText
              name="userName"
              fieldProps={{
                size: 'large',
                prefix: <UserOutlined className={styles.prefixIcon} />,
              }}
              placeholder={intl.formatMessage({
                id: 'pages.login.username.placeholder',
              })}
              rules={[
                {
                  required: true,
                  message: (
                    <FormattedMessage
                      id="pages.login.username.required"
                      defaultMessage="请输入用户名！"
                    />
                  ),
                },
              ]}
            />
        <ProFormText.Password
              name="password"
              fieldProps={{
                size: 'large',
                prefix: <LockTwoTone className={styles.prefixIcon} />,
              }}
              placeholder={intl.formatMessage({
                id: 'pages.login.password.placeholder',
              })}
              rules={[
                {
                  required: true,
                  message: (
                    <FormattedMessage
                      id="pages.login.password.required"
                      defaultMessage="请输入密码！"
                    />
                  ),
                },
              ]}
            />
        <div
          style={{
            marginBottom: 24,
          }}
        >
        </div>
      </ProForm>
      <Button hidden={!ssoEnabled} type="primary" size='large' style={{ width:'100%', marginTop:'20px' }} href='/sso/login' >{ ssoLoginButtonText }</Button>
    </div>
  );
};

export default connect(({ login, loading }: ConnectState) => ({
  userLogin: login,
  submitting: loading.effects['login/login'],
}))(Login);
