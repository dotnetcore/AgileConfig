import { Button, message, Modal, Checkbox, Tooltip } from 'antd';
import Editor from '@monaco-editor/react';
import React, { useState, useEffect } from 'react';
import { getConfigsKvList, saveKvList } from '../service';
import { CheckboxChangeEvent } from 'antd/es/checkbox';
import { QuestionCircleOutlined } from '@ant-design/icons';
import { useIntl } from 'umi';
export type TextEditorProps = {
  appId: string;
  appName: string;
  ModalVisible: boolean;
  env: string;
  onCancel: () => void;
  onSaveSuccess: () => void;
};
const TextEditor: React.FC<TextEditorProps> = (props) => {
  const intl = useIntl();
  const [kvText, setkvText] = useState<string | undefined>('');
  const [isPatch, setIsPatch] = useState<boolean>(false);

  const onIsPatchChange = (e: CheckboxChangeEvent) => {
    setIsPatch(e.target.checked);
  };

  const handleSave = async (
    kvText: string | undefined,
    appId: string,
    env: string,
    isPatch: boolean,
  ) => {
    const hide = message.loading(intl.formatMessage({
      id: 'saving'
    }));
    try {
      const result = await saveKvList(appId, env, kvText ? kvText : '', isPatch);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({
          id: 'save_success'
        }));
      } else {
        message.error(result.message ? result.message : intl.formatMessage({
          id: 'save_fail'
        }));
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({
        id: 'save_fail'
      }));
      return false;
    }
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
      title={intl.formatMessage({
        id: 'pages.configs.textEditor.title'
      })}
      okText={intl.formatMessage({
        id: 'save'
      })}
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
            {intl.formatMessage({
              id: 'pages.configs.textEditor.format'
            })}
          </div>
          <div>
            <span style={{ marginRight: '12px' }}>
              <Checkbox onChange={onIsPatchChange} value={isPatch}>
                {intl.formatMessage({
                  id: 'pages.configs.textEditor.patchMode'
                })}
              </Checkbox>
              <Tooltip title={intl.formatMessage({
                id: 'pages.configs.textEditor.patchModeTooltip'
              })}>
                <QuestionCircleOutlined />
              </Tooltip>
            </span>

            <Button
              onClick={() => {
                props.onCancel();
              }}
            >
              {intl.formatMessage({
                id: 'cancel'
              })}
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
              {intl.formatMessage({
                id: 'save'
              })}
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
