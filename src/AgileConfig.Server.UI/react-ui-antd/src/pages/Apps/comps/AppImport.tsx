import { getIntl, getLocale, useIntl } from 'umi';
import { UploadOutlined } from '@ant-design/icons';
import { Alert, Button, message, Modal, Space, Table, Upload } from 'antd';
import React, { useMemo, useState } from 'react';
import { AppImportFile, AppImportPreviewItem, AppImportPreviewResult } from '../data';
import { importApps, previewImportApps } from '../service';

export type AppImportProps = {
  visible: boolean;
  onCancel: () => void;
  onSuccess: () => void;
};

const AppImport: React.FC<AppImportProps> = ({ visible, onCancel, onSuccess }) => {
  const intl = useIntl();
  const [preview, setPreview] = useState<AppImportPreviewResult>();
  const [importFile, setImportFile] = useState<AppImportFile>();
  const [uploading, setUploading] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const reset = () => {
    setPreview(undefined);
    setImportFile(undefined);
    setUploading(false);
    setSubmitting(false);
  };

  const columns = useMemo(
    () => [
      {
        title: intl.formatMessage({ id: 'pages.app.import.preview.order' }),
        dataIndex: 'order',
        width: 80,
      },
      {
        title: intl.formatMessage({ id: 'pages.app.table.cols.appid' }),
        dataIndex: 'appId',
      },
      {
        title: intl.formatMessage({ id: 'pages.app.table.cols.appname' }),
        dataIndex: 'name',
      },
      {
        title: intl.formatMessage({ id: 'pages.app.table.cols.group' }),
        dataIndex: 'group',
      },
      {
        title: intl.formatMessage({ id: 'pages.app.import.preview.parents' }),
        dataIndex: 'inheritancedApps',
        render: (value: string[]) => value?.join(', ') || '-',
      },
      {
        title: intl.formatMessage({ id: 'pages.app.import.preview.envCount' }),
        dataIndex: 'envCount',
        width: 90,
      },
      {
        title: intl.formatMessage({ id: 'pages.app.import.preview.configCount' }),
        dataIndex: 'configCount',
        width: 110,
      },
    ],
    [intl],
  );

  const handlePreview = async (file: File) => {
    setUploading(true);
    try {
      const text = await file.text();
      const parsedFile = JSON.parse(text) as AppImportFile;
      setImportFile(parsedFile);

      const result = await previewImportApps(file);
      if (result.success) {
        setPreview(result.data);
        message.success(intl.formatMessage({ id: 'pages.app.import.preview.success' }));
      } else {
        setPreview(result.data);
        message.error(result.message || intl.formatMessage({ id: 'pages.app.import.preview.failed' }));
      }
    } catch (error) {
      setPreview(undefined);
      setImportFile(undefined);
      message.error(intl.formatMessage({ id: 'pages.app.import.preview.failed' }));
    } finally {
      setUploading(false);
    }

    return false;
  };

  const handleImport = async () => {
    if (!importFile || !preview?.apps?.length || preview.errors?.length) {
      message.warning(intl.formatMessage({ id: 'pages.app.import.no_valid_preview' }));
      return;
    }

    setSubmitting(true);
    const hide = message.loading(
      getIntl(getLocale()).formatMessage({
        id: 'pages.app.import.importing',
      }),
    );
    try {
      const result = await importApps(importFile);
      hide();
      if (result.success) {
        message.success(intl.formatMessage({ id: 'pages.app.import.success' }));
        reset();
        onSuccess();
      } else {
        message.error(result.message || intl.formatMessage({ id: 'pages.app.import.failed' }));
      }
    } catch (error) {
      hide();
      message.error(intl.formatMessage({ id: 'pages.app.import.failed' }));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      width={1000}
      maskClosable={false}
      title={intl.formatMessage({ id: 'pages.app.import.title' })}
      visible={visible}
      onCancel={() => {
        reset();
        onCancel();
      }}
      onOk={handleImport}
      okButtonProps={{ disabled: !preview?.apps?.length || !!preview?.errors?.length, loading: submitting }}
      destroyOnClose
    >
      <Space direction="vertical" style={{ width: '100%' }} size="middle">
        <Alert type="info" showIcon message={intl.formatMessage({ id: 'pages.app.import.tip' })} />
        <Upload accept=".json" beforeUpload={handlePreview} showUploadList={false} disabled={uploading || submitting}>
          <Button type="primary" icon={<UploadOutlined />} loading={uploading}>
            {intl.formatMessage({ id: 'pages.app.import.select_file' })}
          </Button>
        </Upload>
        {!!preview?.errors?.length && (
          <Alert
            type="error"
            showIcon
            message={intl.formatMessage({ id: 'pages.app.import.validation_failed' })}
            description={
              <div>
                {preview.errors.map((item) => (
                  <div key={item}>{item}</div>
                ))}
              </div>
            }
          />
        )}
        <Table<AppImportPreviewItem>
          rowKey="appId"
          pagination={false}
          columns={columns}
          dataSource={preview?.apps || []}
          locale={{ emptyText: intl.formatMessage({ id: 'pages.app.import.empty' }) }}
        />
      </Space>
    </Modal>
  );
};

export default AppImport;
