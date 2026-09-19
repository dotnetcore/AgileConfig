import { oidcLogin } from '@/services/login';
import { history, useIntl, useLocation } from '@umijs/max';
import { message } from 'antd';
import { useEffect } from 'react';
import { OidcCallback } from './index';

jest.mock('react', () => ({
  ...jest.requireActual('react'),
  useEffect: jest.fn(),
}));

jest.mock('@umijs/max', () => ({
  connect: () => (component) => component,
  history: {
    replace: jest.fn(),
  },
  useIntl: jest.fn(),
  useLocation: jest.fn(),
}));

jest.mock('antd', () => ({
  message: {
    error: jest.fn(),
    success: jest.fn(),
  },
  Spin: () => null,
}));

jest.mock('@ant-design/pro-components', () => ({
  PageContainer: () => null,
}));

jest.mock('@/services/login', () => ({
  oidcLogin: jest.fn(),
}));

describe('OidcCallback', () => {
  it('persists the login state before redirecting after a successful callback', async () => {
    const response = {
      status: 'ok',
      token: 'oidc-token',
      currentAuthority: ['admin'],
      currentFunctions: ['*'],
    };
    const dispatch = jest.fn();
    const formatMessage = jest.fn().mockReturnValue('pages.login.loginsuccess');

    jest.mocked(useIntl).mockReturnValue({ formatMessage });
    jest.mocked(useLocation).mockReturnValue({ search: '?code=authorization-code' });
    jest.mocked(useEffect).mockImplementation((effect) => {
      effect();
    });
    jest.mocked(oidcLogin).mockResolvedValue(response);

    OidcCallback({ dispatch });
    await Promise.resolve();

    expect(oidcLogin).toHaveBeenCalledWith('authorization-code');
    expect(dispatch).toHaveBeenCalledWith({
      type: 'login/changeLoginStatus',
      payload: response,
    });
    expect(message.success).toHaveBeenCalledWith('pages.login.loginsuccess');
    expect(history.replace).toHaveBeenCalledWith('/');
    expect(dispatch.mock.invocationCallOrder[0]).toBeLessThan(
      jest.mocked(history.replace).mock.invocationCallOrder[0],
    );
  });
});
