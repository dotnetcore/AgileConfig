import {  ModalForm, ProFormDependency, ProFormSelect, ProFormSwitch, ProFormText } from "@ant-design/pro-form";
import React from 'react';
import { AppListItem } from "../data";
import { inheritancedApps } from "../service";
export type UpdateFormProps = {
    onSubmit: (values: AppListItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    value: AppListItem ;
    setValue: React.Dispatch<React.SetStateAction<AppListItem | undefined>>
  };
const UpdateForm : React.FC<UpdateFormProps> = (props)=>{

    return (
    <ModalForm 
    title="编辑应用"
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
    readonly={true}
      label="ID"
      name="id"
    />
    <ProFormText.Password
      label="密钥"
      name="secret"
    />
    <ProFormSwitch 
      tooltip="公共应用可以被其他应用关联"
      label="公共应用" 
      name="inheritanced"
      >
    </ProFormSwitch>
    <ProFormDependency name={
      ["inheritanced"]
    }>
      {
        (e) => {
          return !e.inheritanced? 
                <ProFormSelect 
                label="关联应用" 
                tooltip="关联后可以读取公共应用的配置项"
                name="inheritancedApps"
                    mode="multiple" 
                    request={async () => {
                      const result = await inheritancedApps();
                      return result.data.map( (x: { name: string, id: string })=> {
                        return { lable:x.name, value:x.id};
                      });
                    }}
                ></ProFormSelect> : null
        }
      }
    </ProFormDependency>
   
    <ProFormSwitch label="启用" name="enabled" 
    ></ProFormSwitch>
    </ModalForm>
    );
}

export default UpdateForm;
