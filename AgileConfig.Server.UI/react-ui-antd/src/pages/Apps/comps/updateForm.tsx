import { FormInstance, ModalForm, ProFormSelect, ProFormSwitch, ProFormText } from "@ant-design/pro-form";
import React, { useRef } from 'react';
import { AppListItem } from "../data";
export type UpdateFormProps = {
    onSubmit: (values: AppListItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    values: Partial<AppListItem>;
  };
const UpdateForm : React.FC<UpdateFormProps> = (props)=>{
  const fromRef = useRef<FormInstance>();
    return (
    <ModalForm 
    formRef={fromRef}
    title="编辑应用" 
    visible={props.updateModalVisible}
    onFinish={
      async (value) => {
        props.onSubmit(value as AppListItem);
      }
    }
    modalProps={
      {
        onCancel: ()=>{
          console.log('modal close ', props.values);
          fromRef.current?.resetFields();
          props.onCancel();
        }
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
      fieldProps={
        {
          value: props.values?.name
        }
      }
    />
    <ProFormText
      rules={[
        {
          required: true,
        },
      ]}
      label="ID"
      name="id"
      fieldProps={
        {
          value: props.values?.id
        }
      }
    />
    <ProFormText.Password
      rules={[
        {
        },
      ]}
      label="密钥"
      name="password"
      fieldProps={
        {
          value: props.values?.secret
        }
      }
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
    <ProFormSwitch label="启用" name="enabled" 
    ></ProFormSwitch>
    </ModalForm>
    );
}

export default UpdateForm;
