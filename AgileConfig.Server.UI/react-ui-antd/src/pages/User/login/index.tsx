import {
  LockTwoTone,
} from '@ant-design/icons';
import { Alert, Tabs } from 'antd';
import React, { useEffect, useState } from 'react';
import ProForm, { ProFormText } from '@ant-design/pro-form';
import { useIntl, connect, FormattedMessage } from 'umi';
import type { Dispatch } from 'umi';
import { history } from 'umi';
import type { StateType } from '@/models/login';
import type { LoginParamsType } from '@/services/login';
import type { ConnectState } from '@/models/connect';
import { PasswordInited } from './service'
import styles from './index.less';

export type LoginProps = {
  dispatch: Dispatch;
  userLogin: StateType;
  submitting?: boolean;
};

const Login: React.FC<LoginProps> = (props) => {
  const { userLogin = {}, submitting } = props;
  const [type, setType] = useState<string>('account');
  const intl = useIntl();

  useEffect(()=>{
    PasswordInited().then(resp=> {
      if (!resp.data) {
        history.replace('/user/initpassword');
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
    </div>
  );
};

export default connect(({ login, loading }: ConnectState) => ({
  userLogin: login,
  submitting: loading.effects['login/login'],
}))(Login);
