import { useIntl } from "@/.umi/plugin-locale/localeExports";
import {  ModalForm, ProFormDependency, ProFormSelect, ProFormSwitch, ProFormText } from "@ant-design/pro-form";
import React, { useEffect, useState } from 'react';
import { AppListItem } from "../data";
import { getAppGroups, inheritancedApps } from "../service";
import { adminUsers } from '@/pages/User/service';
import { Divider, Input } from "antd";
import { PlusOutlined } from "@ant-design/icons";

export type UpdateFormProps = {
    onSubmit: (values: AppListItem) => Promise<void>;
    onCancel: () => void;
    updateModalVisible: boolean;
    value: AppListItem | undefined ;
    setValue: React.Dispatch<React.SetStateAction<AppListItem | undefined>>
  };
const UpdateForm : React.FC<UpdateFormProps> = (props)=>{
    const intl = useIntl();
    const [appGroups, setAppGroups] = useState<{label:string, value:string}[]>([]);
    const [newAppGroupName, setNewAppGroupName] = useState<string>('');
    useEffect(()=>{
      getAppGroups().then(x=>{
        if (x.success) {
          const groups:{label:string, value:string}[] = [];
          x.data.forEach((i: any)=>{
            groups.push({
              label:i,
              value: i
            });
          })
          setAppGroups(groups);
        } 
      })
    },[]);
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
          <ProFormSelect
                placeholder="应用所属的组"
                  label="应用组"
                  name="group"
                  options={appGroups}
                  fieldProps={{
                    dropdownRender: (menu) => (
                      <div>
                      {menu}
                      <Divider style={{ margin: '4px 0' }} />
                        <div style={{ display: 'flex', flexWrap: 'nowrap', padding: 8 }}>
                          <Input placeholder="输入组名" style={{ flex: 'auto' }} value={newAppGroupName} onChange={(e)=>{ setNewAppGroupName(e.target.value) }} />
                          <a
                            style={{ flex: 'none', padding: '8px', display: 'block', cursor: 'pointer' }}
                            onClick={()=>{
                              if(newAppGroupName){
                                setAppGroups([...appGroups, {
                                  label: newAppGroupName,
                                  value: newAppGroupName
                                }]);
                                setNewAppGroupName('');
                              }
                            }}
                          >
                            <PlusOutlined /> 
                          </a>
                        </div>
                      </div>
                    )
                  }}
        ></ProFormSelect>
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
    <ProFormSelect
              rules={[
                {
                  required: true,
                },
              ]}
                  label="管理员"
                  name="appAdmin"
                  request={async () => {
                    const result = await adminUsers();
                    return result.data.map( (x: { userName: string, id: string, team:string })=> {
                      console.log(x);
                      return { label:x.userName + ' - ' + (x.team?x.team:''), value:x.id};
                    });
                  }}
        ></ProFormSelect>
    <ProFormSwitch label={
      intl.formatMessage({
        id: 'pages.app.form.enabled'
      })
    } name="enabled" checkedChildren={true} unCheckedChildren={false}></ProFormSwitch>
    </ModalForm>
    );
}

export default UpdateForm;
