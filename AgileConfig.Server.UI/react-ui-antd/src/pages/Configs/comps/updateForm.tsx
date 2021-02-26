import {  ModalForm, ProFormText, ProFormTextArea } from "@ant-design/pro-form";
import React from 'react';
import { ConfigListItem } from "../data";
export type UpdateFormProps = {
    appId: string,
    appName: string,
    onSubmit: (values: ConfigListItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    value: ConfigListItem | undefined ;
    setValue: React.Dispatch<React.SetStateAction<ConfigListItem | undefined>>
  };
const UpdateForm : React.FC<UpdateFormProps> = (props)=>{

    return (
    <ModalForm 
    title="编辑配置"
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
          readonly={true}
          name="id"
          hidden={true}
        />
        <ProFormText
          initialValue={props.appName}
          rules={[
            {
            },
          ]}
          readonly={true}
          label="应用"
          name="appName"
        />
        <ProFormText
          rules={[
            {
            },
          ]}
          readonly={true}
          label="appId"
          name="appId"
          hidden={true}
        />
        <ProFormText
          rules={[
            {
            },
          ]}
          label="组"
          name="group"
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label="键"
          name="key"
        />
        <ProFormTextArea
          rules={[
            {
              required: true,
            },
          ]}
          label="值"
          name="value"
          fieldProps={
            {
              autoSize:{
                minRows: 3, maxRows: 8
              }
            }
          }
        />
        <ProFormTextArea
          rules={[
            {
            },
          ]}
          label="描述"
          name="description"
        />
    
    </ModalForm>
    );
}

export default UpdateForm;
