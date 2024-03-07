import { Client, fetchExchange, gql } from '@urql/core';
import { authExchange } from '@urql/exchange-auth';

const AUTHENTICATE_QUERY = gql`
  mutation ($input: AuthenticateInput!) {
    authenticate(input: $input) {
      accessToken
    }
  }
`;

export const api = (baseUrl: string) => {
  const client = new Client({
    url: `${baseUrl}/graphql`,
    exchanges: [
      authExchange((utils) => {
        let token: string | undefined;

        return Promise.resolve({
          addAuthToOperation(operation) {
            if (!token) {
              return operation;
            }

            return utils.appendHeaders(operation, {
              Authorization: `Bearer ${token}`,
            });
          },
          didAuthError(error) {
            return error.graphQLErrors.some(
              (e) => e.extensions?.code === 'AUTH_NOT_AUTHORIZED'
            );
          },
          async refreshAuth() {
            const result = await utils.mutate<{
              authenticate: { accessToken?: string };
            }>(AUTHENTICATE_QUERY, {
              input: {
                email: 'e2e-user@example.com',
                password: 'e2e-user-pass',
              },
            });

            token = result.data?.authenticate.accessToken;
          },
          willAuthError() {
            return !token;
          },
        });
      }),
      fetchExchange,
    ],
  });

  return {
    client,
  };
};
