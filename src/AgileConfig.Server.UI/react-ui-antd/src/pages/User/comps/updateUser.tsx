import { useIntl } from "@/.umi/plugin-locale/localeExports";
import { getAuthority } from "@/utils/authority";
import {  ModalForm,  ProFormSelect,  ProFormText } from "@ant-design/pro-form";
import React from 'react';
import { UserItem } from "../data";
export type UpdateUserProps = {
    onSubmit: (values: UserItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    value: UserItem | undefined ;
    setValue: React.Dispatch<React.SetStateAction<UserItem | undefined>>
  };
const UpdateForm : React.FC<UpdateUserProps> = (props)=>{
    const intl = useIntl();
    const hasUserRole = (role:string) => {
      const authority = getAuthority();
      if (Array.isArray(authority)) {
        if (authority.find(x=> x === role)) {
          return true;
        }
      }
    
      return false;
    }
    return (
    <ModalForm 
    width="400px"
    title="编辑用户"
    initialValues={props.value}
    visible={props.updateModalVisible}
    modalProps={
      {
        onCancel: ()=>{
          props.onCancel();
        }
      }
    }
     onFinish={
       props.onSubmit
    }
    >
    <ProFormText
          label= "id"
          name="id" 
          hidden={true}
          readonly={true}
        />
   <ProFormText
          label= "用户名"
          name="userName" 
          readonly={true}
        />
 
       <ProFormText
          label= "团队"
          name="team" 
        />
    <ProFormSelect
        rules={[
          {
            required: true,
          },
        ]}
        label="角色"
        name="userRoles"
        mode="multiple" 
        options = {
          hasUserRole('SuperAdmin')?[
            {
              value: 1,
              label: '管理员',
            },
            {
              value: 2,
              label: '操作员',
            }
          ]:[{
            value: 2,
            label: '操作员',
          }]
        }
      >
        </ProFormSelect> 
    </ModalForm>
    );
}

export default UpdateForm;
