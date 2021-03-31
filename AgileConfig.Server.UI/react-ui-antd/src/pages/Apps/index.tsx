import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm,  ProFormDependency, ProFormSelect, ProFormSwitch, ProFormText } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message, Modal, Space, Switch, Tag } from 'antd';
import React, { useState, useRef } from 'react';
import {getIntl, getLocale, Link, useIntl} from 'umi';
import UpdateForm from './comps/updateForm';
import { AppListItem, AppListParams, AppListResult } from './data';
import { addApp, editApp, delApp, queryApps, inheritancedApps,enableOrdisableApp } from './service';

const { confirm } = Modal;

const handleAdd = async (fields: AppListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'saving'
  }));
  try {
    const result = await addApp({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id:'save_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id:'save_fail'
    }));
    return false;
  }
};
const handleEdit = async (app: AppListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'saving'
  }));
  try {
    const result = await editApp({ ...app });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id:'save_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id:'save_fail'
    }));
    return false;
  }
};
const handleDel = async (fields: AppListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'deleting'
  }));
  try {
    const result = await delApp({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id:'delete_success'
      }));
    } else {
      message.error(intl.formatMessage({
        id:'delete_fail'
      }));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id:'delete_fail'
    }));
    return false;
  }
};

const appList: React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
 
  const intl = useIntl();

  const [createModalVisible, setCreateModalVisible] = useState<boolean>(false);
  const [updateModalVisible, setUpdateModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<AppListItem>();
  const [dataSource, setDataSource] = useState<AppListResult>();
  const handleQuery = async (params: AppListParams) => {
    const result = await queryApps(params);
    setDataSource(result);
    return result;
  }
  const handleEnabledChange =async (checked:boolean, entity:AppListItem) => {
    const result = await enableOrdisableApp(entity.id);
    const qiyong = intl.formatMessage({
      id: 'enabled.1'
    });
    const jinyong = intl.formatMessage({
      id: 'enabled.0'
    });
    const success = intl.formatMessage({
      id: 'success'
    });
    const failed = intl.formatMessage({
      id: 'failed'
    });
    if (result.success) {
      const msg = (checked?qiyong:jinyong) + success;
      message.success(msg);
      if (dataSource) {
        const app = dataSource?.data?.find(x=>x.id === entity.id);
        if (app) {
          app.enabled = checked;
        }
        setDataSource({...dataSource});
      }
    }
    else{
      message.error((checked?qiyong:jinyong) + failed)
    }
  }
  const columns: ProColumns<AppListItem>[] = [
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.appname'
      }),
      dataIndex: 'name',
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.appid'
      }),
      dataIndex: 'id',
      copyable: true,
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.secret'
      }),
      dataIndex: 'secret',
      valueType: 'password',
      hideInSearch: true,
      copyable: true,
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.create_time'
      }),
      dataIndex: 'createTime',
      valueType: 'dateTime',
      hideInSearch: true
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.update_time'
      }),
      dataIndex: 'updateTime',
      valueType: 'dateTime',
      hideInSearch: true
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.public'
      }),
      dataIndex: 'inheritanced',
      hideInSearch: true,
      valueEnum: {
        false: {
          text:  intl.formatMessage({
            id:'pages.app.inheritanced.false'
          }),
          status: 'default'
        },
        true: {
          text:  intl.formatMessage({
            id:'pages.app.inheritanced.true'
          }),
          status: 'success'
        }
      }
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.link'
      }),
      dataIndex: 'inheritancedApps',
      search: false,
      renderFormItem: (_, { defaultRender }) => {
        return defaultRender(_);
      },
      render: (_, record) => (
        <Space>
          {record.inheritancedAppNames?.map((name:string) => (
            <Tag color="blue" key={name}>
              {name}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.enabled'
      }),
      dataIndex: 'enabled',
      render: (dom, entity) => {
        return <Switch checked={entity.enabled} size="small" onChange={
          (e)=>{
            handleEnabledChange(e, entity);
          }
           }/>
      },
      hideInSearch: true
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.action'
      }),
      valueType: 'option',
      render: (text, record, _, action) => [
        <Link 
        to={
          {
            pathname:'/app/config/' + record.id + '/' + record.name,
          }
        }
      >{intl.formatMessage({
        id:'pages.app.table.cols.action.configs'
      })}</Link>,
        <a
          onClick={() => {
            setUpdateModalVisible(true);
            setCurrentRow(record);
            console.log('select app ', record);
            console.log('current app ', currentRow);
          }}
        >
          {
            intl.formatMessage({
              id:'pages.app.table.cols.action.edit'
            })
          }
        </a>,
        <Button type="link" danger
          onClick={() => {
            const msg = intl.formatMessage({
              id:'pages.app.delete_msg'
            }) + `【${record.name}】?`;
            confirm({
              icon: <ExclamationCircleOutlined />,
              content: msg,
              async onOk() {
                console.log('delete app ' + record.name);
                const success = await handleDel(record);
                if (success) {
                  actionRef.current?.reload();
                }
              },
              onCancel() {
                console.log('Cancel');
              },
            });
          }}
        >
          {
            intl.formatMessage({
              id:'pages.app.table.cols.action.delete'
            })
          }
        </Button>
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable
        actionRef={actionRef}
        options={
          false
        }
        search={{
          labelWidth: 'auto',
        }}
        rowKey="id"
        columns={columns}
        request={(params, sorter, filter) => handleQuery(params)}
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary" onClick={() => { setCreateModalVisible(true) }}>
            {
              intl.formatMessage({
                id:'pages.app.table.cols.action.add'
              })
            }
          </Button>
        ]}
        //dataSource={dataSource}
      />
      <ModalForm
        formRef={addFormRef}
        title={
          intl.formatMessage({
            id: 'pages.app.form.title.add'
          })
        }
        visible={createModalVisible}
        onVisibleChange={setCreateModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as AppListItem);
            if (success) {
              setCreateModalVisible(false);
              if (actionRef.current) {
                actionRef.current.reload();
              }
            }
            addFormRef.current?.resetFields();
          }
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
          label={
            intl.formatMessage({
              id: 'pages.app.form.id'
            })
          }
          name="id"
        />
        <ProFormText.Password
          rules={[
            {
            },
          ]}
          label={
            intl.formatMessage({
              id: 'pages.app.form.secret'
            })
          }
          name="secret"
        />
        <ProFormSwitch tooltip={
          intl.formatMessage({
            id: 'pages.app.form.public.tooltip'
          })
        } label={
           intl.formatMessage({
            id: 'pages.app.form.public'
          })
        } name="inheritanced" checkedChildren={true} unCheckedChildren={false}></ProFormSwitch>
        <ProFormDependency name={
          ["inheritanced"]
        }>
          {
            (e) => {
              return !e.inheritanced ?
                <ProFormSelect
                  tooltip={
                    intl.formatMessage({
                      id: 'pages.app.form.connected.tooltip'
                    })
                  }
                  label={
                    intl.formatMessage({
                      id: 'pages.app.form.connected'
                    })
                  }
                  name="inheritancedApps"
                  mode="multiple" 
                  request={async () => {
                    const result = await inheritancedApps('');
                    return result.data.map( (x: { name: string, id: string })=> {
                      console.log(x);
                      return { label:(x.name + x.id), value:x.id};
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
        } name="enabled" initialValue={true} checkedChildren={true} unCheckedChildren={false}>
        </ProFormSwitch>
      </ModalForm>
      {
        updateModalVisible &&
        <UpdateForm
          value={currentRow}
          setValue={setCurrentRow}
          updateModalVisible={updateModalVisible}
          onCancel={
            () => {
              setCurrentRow(undefined);
              setUpdateModalVisible(false);
              console.log('set currentrow undefined');
            }
          }
          onSubmit={
            async (value) => {
              setCurrentRow(undefined);
              const success = await handleEdit(value);
              if (success) {
                setUpdateModalVisible(false);
                if (actionRef.current) {
                  actionRef.current.reload();
                }
              }
              addFormRef.current?.resetFields();
            }
          }/>
      }
    </PageContainer>
  );
}
export default appList;
