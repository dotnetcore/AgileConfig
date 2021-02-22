import { ModalForm, ProFormSelect, ProFormSwitch, ProFormText } from "@ant-design/pro-form";
import React from "react";
export type UpdateFormProps = {
    onSubmit: (values: any) => Promise<void>;
    updateModalVisible: boolean;
    handleModalVisible: React.Dispatch<React.SetStateAction<boolean>>;
  };
const UpdateForm : React.FC<UpdateFormProps> = (props)=>{
    return (
    <ModalForm 
    title="编辑应用" 
    visible={props.updateModalVisible}
    onVisibleChange={props.handleModalVisible}
    onFinish={
      async (value) => {
        props.onSubmit(value);
      }
    }
    >
    <ProFormText
      rules={[
        {
          required: true,
        },
      ]}
      label="名称"
      name="name"
    />
    <ProFormText
      rules={[
        {
          required: true,
        },
      ]}
      label="ID"
      name="id"
    />
    <ProFormText.Password
      rules={[
        {
        },
      ]}
      label="密钥"
      name="password"
    />
    <ProFormSwitch label="公共应用" name="inheritanced"></ProFormSwitch>
    <ProFormSelect label="关联应用" name="inheritancedApps"
    mode="multiple" 
    request={async () => [
      { label: 'app1', value: '1' },
      { label: 'app2', value: '2' },
      { label: 'app3', value: '3' },
      { label: 'app4', value: '4' },
    ]}
    ></ProFormSelect>
    <ProFormSwitch label="启用" name="enabled"></ProFormSwitch>
    </ModalForm>
    );
}

export default UpdateForm;
