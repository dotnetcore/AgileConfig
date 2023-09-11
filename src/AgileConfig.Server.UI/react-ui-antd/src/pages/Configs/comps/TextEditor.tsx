import { Button, message, Modal, Input, Checkbox, Tooltip } from 'antd';
const { TextArea } = Input;
import Editor from '@monaco-editor/react';
import React, { useState, useEffect } from 'react';
import { getConfigsKvList, saveKvList } from '../service';
import { CheckboxChangeEvent } from 'antd/es/checkbox';
import { QuestionCircleOutlined } from '@ant-design/icons';
export type TextEditorProps = {
  appId: string;
  appName: string;
  ModalVisible: boolean;
  env: string;
  onCancel: () => void;
  onSaveSuccess: () => void;
};
const handleSave = async (
  kvText: string | undefined,
  appId: string,
  env: string,
  isPatch: boolean,
) => {
  const hide = message.loading('正在保存...');
  try {
    const result = await saveKvList(appId, env, kvText ? kvText : '', isPatch);
    hide();
    const success = result.success;
    if (success) {
      message.success('保存成功！');
    } else {
      message.error(result.message ? result.message : '保存失败！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('保存失败！');
    return false;
  }
};
const TextEditor: React.FC<TextEditorProps> = (props) => {
  const [kvText, setkvText] = useState<string | undefined>('');
  const [isPatch, setIsPatch] = useState<boolean>(false);

  const onIsPatchChange = (e: CheckboxChangeEvent) => {
    setIsPatch(e.target.checked);
  };
  useEffect(() => {
    getConfigsKvList(props.appId, props.env).then((res) => {
      if (res.success) {
        const list = res.data.map((x: { value: string; key: string }) => x.key + '=' + x.value);
        setkvText(list.join('\n'));
      }
    });
  }, []);

  return (
    <Modal
      maskClosable={false}
      title="按 TEXT 视图编辑"
      okText="保存"
      width={800}
      visible={props.ModalVisible}
      onCancel={() => {
        props.onCancel();
      }}
      footer={
        <div style={{ display: 'flex', justifyContent: 'space-between' }}>
          <div
            style={{
              color: '#999',
              fontSize: '12px',
            }}
          >
            严格按照 KEY=VALUE 格式编辑，每行一个配置
          </div>
          <div>
            <span style={{ marginRight: '12px' }}>
              <Checkbox onChange={onIsPatchChange} value={isPatch}>
                补丁模式更新
              </Checkbox>
              <Tooltip title="补丁模式,只会新增或修改配置,上面未包含的现有配置项不会被删除">
                <QuestionCircleOutlined />
              </Tooltip>
            </span>

            <Button
              onClick={() => {
                props.onCancel();
              }}
            >
              取消
            </Button>
            <Button
              type="primary"
              onClick={async () => {
                const saveResult = await handleSave(kvText, props.appId, props.env, isPatch);
                if (saveResult) {
                  props.onSaveSuccess();
                }
              }}
            >
              保存
            </Button>
          </div>
        </div>
      }
    >
      <Editor
        height="500px"
        defaultLanguage="text"
        defaultValue=""
        value={kvText}
        options={{ minimap: { enabled: false } }}
        onChange={(v, e) => {
          setkvText(v);
        }}
      />
    </Modal>
  );
};

export default TextEditor;
