const { createConfig } = require('@umijs/test');

const baseConfig = createConfig();

module.exports = {
  ...baseConfig,
  verbose: false,
  moduleNameMapper: {
    ...baseConfig.moduleNameMapper,
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  globals: {
    ANT_DESIGN_PRO_ONLY_DO_NOT_USE_IN_YOUR_PRODUCTION: false,
    localStorage: null,
  },
};
