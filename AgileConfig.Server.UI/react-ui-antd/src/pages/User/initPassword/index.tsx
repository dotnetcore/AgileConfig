import {
    LockTwoTone,
  } from '@ant-design/icons';
  import { Alert, message, Tabs } from 'antd';
  import React, { useEffect, useState } from 'react';
  import ProForm, { ProFormText } from '@ant-design/pro-form';
  import { useIntl, connect, FormattedMessage,history, getIntl, getLocale } from 'umi';

  import styles from './index.less';
import { InitPasswordModel } from './data';
import { initPassword } from './service';
  
  const handleSave = async function name(params:InitPasswordModel) {
    const intl = getIntl(getLocale());
    const hide = message.loading(intl.formatMessage({
      id: 'saving'
    }));
    try {
      const result = await initPassword(params);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({
          id: 'pages.initpassword.init_success'
        }));
        history.replace('/user/login');
      } else {
        message.error(result.message);
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({
        id: 'save_fail'
      }));
      return false;
    }
  }

  const InitPassword: React.FC = (props) => {
    const intl = useIntl();
    useEffect(()=>{
        const msg = intl.formatMessage({
          id: 'pages.initpassword.init_tip',
        })
        message.info(msg);
    },[])
    return (
      <div className={styles.main}>
        <ProForm
          initialValues={{
            autoLogin: true,
          }}
          submitter={{
            render: (_, dom) => dom.pop(),
            submitButtonProps: {
              size: 'large',
              style: {
                width: '100%',
              },
            },
            searchConfig: {
              submitText: intl.formatMessage({
                id: 'pages.initpassword.submit',
              })}
          }}
          onFinish={async (values) => {
             const result = handleSave(values as InitPasswordModel)
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
              id: 'pages.initpassword.tab',
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
               <ProFormText.Password
               name="confirmPassword"
               fieldProps={{
                 size: 'large',
                 prefix: <LockTwoTone className={styles.prefixIcon} />,
               }}
               placeholder={intl.formatMessage({
                id: 'pages.initpassword.ag_password.placeholder',
              })}
               rules={[
                 {
                   required: true,
                   message: (
                     <FormattedMessage
                       id="pages.login.password.required"
                       defaultMessage="请再次输入密码！"
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
  
  export default InitPassword;