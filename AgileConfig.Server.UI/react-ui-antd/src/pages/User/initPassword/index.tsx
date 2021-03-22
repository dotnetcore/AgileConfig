import {
    LockTwoTone,
  } from '@ant-design/icons';
  import { Alert, message, Tabs } from 'antd';
  import React, { useEffect, useState } from 'react';
  import ProForm, { ProFormText } from '@ant-design/pro-form';
  import { useIntl, connect, FormattedMessage,history } from 'umi';

  import styles from './index.less';
import { InitPasswordModel } from './data';
import { initPassword } from './service';
  
  const handleSave = async function name(params:InitPasswordModel) {
    const hide = message.loading('正在初始化');
    try {
      const result = await initPassword(params);
      hide();
      const success = result.success;
      if (success) {
        message.success('初始化成功，请使用管理员密码重新登录。');
        history.replace('/user/login');
      } else {
        message.error(result.message);
      }
      return success;
    } catch (error) {
      hide();
      message.error('初始化失败请重试！');
      return false;
    }
  }

  const InitPassword: React.FC = (props) => {

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
              submitText: '确定'
            }
          }}
          onFinish={async (values) => {
             const result = handleSave(values as InitPasswordModel)
          }}
        >
          <Tabs activeKey="account">
            <Tabs.TabPane
              key="account"
              tab="初始化管理员密码"
            />
          </Tabs>
          <ProFormText.Password
               name="password"
               fieldProps={{
                 size: 'large',
                 prefix: <LockTwoTone className={styles.prefixIcon} />,
               }}
               placeholder="请输入密码"
               rules={[
                 {
                   required: true,
                   message: (
                     <FormattedMessage
                       id="pages.login.initpassword.required"
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
               placeholder="请再次输入密码"
               rules={[
                 {
                   required: true,
                   message: (
                     <FormattedMessage
                       id="pages.login.initpassword.required"
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