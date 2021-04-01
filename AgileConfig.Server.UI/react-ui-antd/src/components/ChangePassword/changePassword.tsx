import request from "@/utils/request";
import { ModalForm, ProFormText } from "@ant-design/pro-form";
import {  message } from "antd";
import React from 'react';
import { useIntl } from "react-intl";

export type ChangepasswordProps = {
    changePasswordModalVisible: boolean;
    onCancel: () => void;
    onSuccess: ()=> void;
  };

const Changepassword : React.FC<ChangepasswordProps> = (props)=>{
    const intl = useIntl();
    const handleChangePassword = async (params:any) => {
        var data = await request('/admin/changepassword', {
            method: 'POST',
            data: params
          })
        if (data.success) {
            message.success(intl.formatMessage({id: 'resetpassword.update_success'}));
            props.onSuccess();
        }else{
            message.error(intl.formatMessage({id: data.err_code}));
        }
    }
    return (
        <ModalForm 
        title={ intl.formatMessage({id: 'resetpassword.title'}) }
        width={400}
        visible={props.changePasswordModalVisible}
        modalProps={
          {
            onCancel: ()=>{
              props.onCancel();
            }
          }
        }
         onFinish={
            handleChangePassword
        }
        >
         
         <ProFormText.Password
              rules={[
                {
                  required: true,
                },
              ]}
              label={intl.formatMessage({id: 'resetpassword.oldpassword'})}
              name="oldPassword"
            />
             <ProFormText.Password
              rules={[
                {
                  required: true,
                },
              ]}
              label={intl.formatMessage({id: 'resetpassword.newpassword'})}
              name="password"
            />
             <ProFormText.Password
              rules={[
                {
                  required: true,
                },
              ]}
              label={intl.formatMessage({id: 'resetpassword.newpassword_ag'})}
              name="confirmPassword"
            />
        
        </ModalForm>
    );
}

export default Changepassword;