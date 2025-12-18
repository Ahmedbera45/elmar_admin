import { defineConfig } from 'orval';

export default defineConfig({
  api: {
    output: {
      mode: 'tags-split',
      target: 'lib/api/generated.ts',
      schemas: 'lib/api/model',
      client: 'react-query',
      mock: false,
      override: {
        mutator: {
          path: 'lib/axios-instance.ts',
          name: 'customInstance',
        },
      },
    },
    input: {
      target: 'http://localhost:5000/swagger/v1/swagger.json',
    },
  },
});
