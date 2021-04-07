import {  ModalForm, ProFormText, ProFormTextArea } from "@ant-design/pro-form";
import React from 'react';
import { useIntl } from "react-intl";
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

    const intl = useIntl();

    return (
    <ModalForm 
    title={intl.formatMessage({id:'pages.configs.from.add.title'})}
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
          label={intl.formatMessage({id:'pages.configs.from.add.app'})}
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
          label={intl.formatMessage({id:'pages.configs.table.cols.g'})}
          name="group"
          readonly={true}
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label={intl.formatMessage({id:'pages.configs.table.cols.k'})}
          name="key"
          readonly={true}
        />
        <ProFormTextArea
          rules={[
            {
              required: true,
            },
          ]}
          label={intl.formatMessage({id:'pages.configs.table.cols.v'})}
          name="value"
          fieldProps={
            {
              autoSize:{
                minRows: 3, maxRows: 12
              }
            }
          }
        />
        <ProFormTextArea
          rules={[
            {
            },
          ]}
          label={intl.formatMessage({id:'pages.configs.table.cols.desc'})}
          name="description"
        />
    
    </ModalForm>
    );
}

export default UpdateForm;
