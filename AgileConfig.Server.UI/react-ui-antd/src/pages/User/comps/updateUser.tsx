import { useIntl } from "@/.umi/plugin-locale/localeExports";
import {  ModalForm,  ProFormText } from "@ant-design/pro-form";
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
          width="md"
          name="id" 
          hidden={true}
          readonly={true}
        />
   <ProFormText
          label= "用户名"
          width="md"
          name="userName" 
          readonly={true}
        />
 
       <ProFormText
          label= "团队"
          width="md"
          name="team" 
        />

    </ModalForm>
    );
}

export default UpdateForm;
