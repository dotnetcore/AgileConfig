import { useIntl } from "@/.umi/plugin-locale/localeExports";
import {  ModalForm, ProFormDependency, ProFormSelect, ProFormSwitch, ProFormText } from "@ant-design/pro-form";
import React from 'react';
import { AppListItem } from "../data";
import { inheritancedApps } from "../service";
export type UpdateFormProps = {
    onSubmit: (values: AppListItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    value: AppListItem | undefined ;
    setValue: React.Dispatch<React.SetStateAction<AppListItem | undefined>>
  };
const UpdateForm : React.FC<UpdateFormProps> = (props)=>{
    const intl = useIntl();

    return (
    <ModalForm 
    title={
      intl.formatMessage({
        id: 'pages.app.form.title.edit'
      })
    }
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
      label={
        intl.formatMessage({
          id: 'pages.app.form.name'
        })
      }
      name="name"
    />
    <ProFormText
     rules={[
      {
        required: true,
      },
    ]}
    readonly={true}
      label={
        intl.formatMessage({
          id: 'pages.app.form.id'
        })
      }
      name="id"
    />
    <ProFormText.Password
      label={
        intl.formatMessage({
          id: 'pages.app.form.secret'
        })
      }
      name="secret"
    />
    <ProFormSwitch 
      tooltip="公共应用可以被其他应用关联"
      label={
        intl.formatMessage({
          id: 'pages.app.form.public'
        })
      } 
      name="inheritanced"
      checkedChildren={true} unCheckedChildren={false}
      >
    </ProFormSwitch>
    <ProFormDependency name={
      ["inheritanced"]
    }>
      {
        (e) => {
          return !e.inheritanced? 
                <ProFormSelect 
                label={intl.formatMessage({
                  id: 'pages.app.form.connected'
                })} 
                tooltip="关联后可以读取公共应用的配置项"
                name="inheritancedApps"
                    mode="multiple" 
                    request={async () => {
                      const result = await inheritancedApps(props.value? props.value.id: '');
                      return result.data.map( (x: { name: string, id: string })=> {
                        return { label:x.name, value:x.id};
                      });
                    }}
                ></ProFormSelect> : null
        }
      }
    </ProFormDependency>
   
    <ProFormSwitch label={
      intl.formatMessage({
        id: 'pages.app.form.enabled'
      })
    } name="enabled" checkedChildren={true} unCheckedChildren={false}></ProFormSwitch>
    </ModalForm>
    );
}

export default UpdateForm;
