import { PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { queryApps } from '../Apps/service';
import { ConfigListItem } from './data';
import { queryConfigs, onlineConfig, offlineConfig } from './service';
const { confirm } = Modal;

const handleOnline = async (fields: ConfigListItem) => {
  const hide = message.loading('正在上线');
  try {
    const result = await onlineConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('上线成功');
    } else {
      message.error('上线失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('上线失败请重试！');
    return false;
  }
};

const handleOffline = async (fields: ConfigListItem) => {
  const hide = message.loading('正在下线');
  try {
    const result = await offlineConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('下线成功');
    } else {
      message.error('下线失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('下线失败请重试！');
    return false;
  }
};

const configs:React.FC = (props:any) => {
  const appId = props.match.params.app_id;
  const appName = props.match.params.app_name;
  const [appEnums, setAppEnums] = useState<any>();
  const actionRef = useRef<ActionType>();
  const getAppEnums = async () =>
  {
    const result = await queryApps({})
    const obj = {};
    result.data.forEach((x)=>{
      obj[x.id] = {
        text: x.name
      }
    });

    return obj;
  }
  useEffect(()=>{
    getAppEnums().then(x=> {
      console.log('app enums ', x);
      setAppEnums({...x});
    });
  }, []);
  const online = (config: ConfigListItem) => {
    confirm({
      content:`确定上线配置【${config.key}】？`,
      onOk: ()=>{
        const result = handleOnline(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const offline = (config: ConfigListItem) => {
    confirm({
      content:`确定下线配置【${config.key}】？`,
      onOk: ()=>{
        const result = handleOffline(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const columns: ProColumns<ConfigListItem>[] = [
    {
      title: '组',
      dataIndex: 'group',
    },
    {
      title: '键',
      dataIndex: 'key',
    },
    {
      title: '值',
      dataIndex: 'value',
    },
    {
      title: '描述',
      dataIndex: 'description',
      hideInSearch: true,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      hideInSearch: true,
      valueType: 'dateTime'
    },
    {
      title: '修改时间',
      dataIndex: 'updateTime',
      hideInSearch: true,
      valueType: 'dateTime'
    },
    {
      title: '状态',
      dataIndex: 'onlineStatus',
      hideInSearch: true,
      valueEnum: {
        0: {
          text:'待上线',
          status: 'warning'
        },
        1: {
          text:'已上线',
          status: 'Success'
        }
      }
    },
    {
      title: '操作',
      valueType: 'option',
      render: (text, record, _, action) => [
        record.onlineStatus? <a onClick={ ()=>{ offline(record)} }>下线</a>:<a onClick={ ()=>{ online(record)} }>上线</a>,
        <a
          key="editable"
          onClick={() => {
            action.startEditable?.(record.id);
          }}
        >
          编辑
        </a>,
        <a>版本历史</a>,
        <Button type="link" danger>
          删除
        </Button>
      ]
    },
  ];
  return (
    <PageContainer>
      <ProTable   
        headerTitle={appName}
        actionRef={actionRef}  
        rowKey="id"
        options={
          false
        }
        columns = {columns}
        search={{
          labelWidth: 'auto',
        }}
        request = { (params, sorter, filter) => queryConfigs() }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary">
            新建
          </Button>,
          <Button key="button" type="primary">
          上线选中的配置
        </Button>,
          <Button key="button" type="primary">
            从json文件导入
          </Button>
        ]}
      />
    </PageContainer>
  );
}
export default configs;
