import request from "@/utils/request";
import { ModalForm, ProFormText } from "@ant-design/pro-form";
import {  message, Modal} from "antd";
import React, { useState } from 'react';

export type ChangepasswordProps = {
    changePasswordModalVisible: boolean;
    onCancel: () => void;
    onSuccess: ()=> void;
  };

const Changepassword : React.FC<ChangepasswordProps> = (props)=>{
    const handleChangePassword = async (params:any) => {
        var data = await request('/admin/changepassword', {
            method: 'POST',
            data: params
          })
        if (data.success) {
            message.success('修改密码成功,请使用新密码重新登录。');
            props.onSuccess();
        }else{
            message.error(data.message);
        }
    }
    return (
        <ModalForm 
        title="修改密码"
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
              label="原密码"
              name="oldPassword"
            />
             <ProFormText.Password
              rules={[
                {
                  required: true,
                },
              ]}
              label="新密码"
              name="password"
            />
             <ProFormText.Password
              rules={[
                {
                  required: true,
                },
              ]}
              label="再次输入新密码"
              name="confirmPassword"
            />
        
        </ModalForm>
    );
}

export default Changepassword;