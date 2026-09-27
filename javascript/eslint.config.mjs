import eslint from '@eslint/js';
import globals from 'globals';
import tseslint from 'typescript-eslint';

/** @type {import('@typescript-eslint/utils').TSESLint.FlatConfig.ConfigFile} */
export default [
    eslint.configs.recommended,
    ...tseslint.configs.recommendedTypeChecked,
    {
        ignores: ['node_modules/**', 'build/**', '*.mjs'],
    },
    {
        files: ['src/**/*.ts', 'test/**/*.ts'],
        languageOptions: {
            parser: tseslint.parser,
            parserOptions: {
                parser: tseslint.parser,
                tsconfigRootDir: import.meta.dirname,
                projectService: true,
            },
            globals: {
                ...globals.browser,
            },
        },
        rules: {
            // js rules
            semi: ['error', 'always'],
            quotes: ['error', 'single'],
            'eol-last': ['error', 'always'],
            'no-trailing-spaces': 'error',
            'no-var': 'error',

            'no-unexpected-multiline': 'warn',
            'prefer-const': 'warn',
            'no-console': 'warn',
            'no-debugger': 'warn',

            'no-unused-vars': 'off', // typescript has its own
            'prefer-spread': 'off',
            'no-case-declarations': 'off',
            'no-undef': 'off',
            'no-extra-boolean-cast': 'off',
            'no-constant-condition': 'off',

            // typescript rules
            '@typescript-eslint/no-unused-vars': [
                'error',
                {
                    vars: 'local',
                    varsIgnorePattern: '^_',
                    args: 'after-used',
                    argsIgnorePattern: '^_',
                    caughtErrors: 'none',
                    caughtErrorsIgnorePattern: '^_',
                    destructuredArrayIgnorePattern: '^_',
                    ignoreRestSiblings: true,
                },
            ],

            '@typescript-eslint/no-deprecated': 'warn',

            '@typescript-eslint/no-explicit-any': 'off',
            '@typescript-eslint/no-non-null-assertion': 'off',
            '@typescript-eslint/no-namespace': 'off',
            '@typescript-eslint/prefer-namespace-keyword': 'off',
            '@typescript-eslint/no-unsafe-assignment': 'off',
            '@typescript-eslint/no-unsafe-return': 'off',
            '@typescript-eslint/no-unsafe-member-access': 'off',
        },
    },
];
