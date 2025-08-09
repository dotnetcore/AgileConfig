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
    title={intl.formatMessage({id: props.value?.id ? 'pages.user.form.title.edit' : 'pages.user.form.title.add'})}
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
          label={intl.formatMessage({id: 'pages.user.form.username'})}
          name="userName" 
          readonly={true}
        />
 
       <ProFormText
          label={intl.formatMessage({id: 'pages.user.form.team'})}
          name="team" 
        />
    <ProFormSelect
        rules={[
          {
            required: true,
          },
        ]}
        label={intl.formatMessage({id: 'pages.user.form.usertype'})}
        name="userRoles"
        mode="multiple" 
        options = {
          hasUserRole('SuperAdmin')?[
            {
              value: 1,
              label: intl.formatMessage({
                id: 'pages.user.role.admin',
              }),
            },
            {
              value: 2,
              label: intl.formatMessage({
                id: 'pages.user.role.operator',
              }),
            }
          ]:[{
            value: 2,
            label: intl.formatMessage({
              id: 'pages.user.role.operator',
            }),
          }]
        }
      >
        </ProFormSelect> 
    </ModalForm>
    );
}

export default UpdateForm;
