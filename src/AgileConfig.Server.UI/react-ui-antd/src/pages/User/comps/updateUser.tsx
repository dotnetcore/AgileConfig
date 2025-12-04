import { useIntl } from "@/.umi/plugin-locale/localeExports";
import {  ModalForm,  ProFormSelect,  ProFormText } from "@ant-design/pro-form";
import React from 'react';
import { UserItem } from "../data";

type RoleOption = {
  value: string;
  label: string;
  code?: string;
  isSystem?: boolean;
};

export type UpdateUserProps = {
    onSubmit: (values: UserItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    value: UserItem | undefined ;
    setValue: React.Dispatch<React.SetStateAction<UserItem | undefined>>;
    roleOptions: RoleOption[];
    defaultRoleIds: string[];
  };
const UpdateForm : React.FC<UpdateUserProps> = (props)=>{
    const intl = useIntl();
    return (
    <ModalForm
    width="400px"
    title={intl.formatMessage({id: props.value?.id ? 'pages.user.form.title.edit' : 'pages.user.form.title.add'})}
    initialValues={{
      ...props.value,
      userRoleIds: props.value?.userRoleIds && props.value.userRoleIds.length > 0 ? props.value.userRoleIds : props.defaultRoleIds
    }}
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
        label={intl.formatMessage({id: 'pages.user.form.userrole'})}
        name="userRoleIds"
        mode="multiple"
        options={props.roleOptions.map(r => ({
          value: r.value,
          label: r.label,
        }))}
      />
    </ModalForm>
    );
}

export default UpdateForm;
