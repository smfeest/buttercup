import { Client, fetchExchange, gql } from '@urql/core';
import { authExchange } from '@urql/exchange-auth';

const AUTHENTICATE_QUERY = gql`
  mutation ($input: AuthenticateInput!) {
    authenticate(input: $input) {
      accessToken
    }
  }
`;

const CREATE_RECIPE_QUERY = gql`
  mutation CreateRecipe($attributes: RecipeAttributesInput!) {
    createRecipe(input: { attributes: $attributes }) {
      recipe {
        id
      }
    }
  }
`;

const DELETE_RECIPE_QUERY = gql`
  mutation DeleteRecipe($id: Long!) {
    deleteRecipe(input: { id: $id }) {
      deleted
    }
  }
`;

const DEFAULT_RECIPE_ATTRIBUTES: RecipeAttributes = {
  title: 'Cheese sandwich',
  ingredients: ['2 slices of bread', 'Butter', 'Cheese'].join('\n'),
  method: [
    'Spread butter on one side of each slice of bread',
    'Place cheese between the slices',
  ].join('\n'),
};

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
              (e) => e.extensions?.code === 'AUTH_NOT_AUTHORIZED',
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
    async createRecipe(
      explicitAttributes: Partial<RecipeAttributes> = {},
    ): Promise<Recipe> {
      const attributes = {
        ...DEFAULT_RECIPE_ATTRIBUTES,
        ...explicitAttributes,
      };

      const result = await client.mutation<{
        createRecipe: { recipe: { id: number } };
      }>(CREATE_RECIPE_QUERY, { attributes });

      if (!result.data) {
        throw new Error('Failed to insert recipe');
      }

      const id = result.data.createRecipe.recipe.id;
      return { id, ...attributes };
    },
    async deleteRecipe(id: number) {
      await client.mutation(DELETE_RECIPE_QUERY, { id });
    },
  };
};

export interface RecipeAttributes {
  title: string;
  preparationMinutes?: number;
  cookingMinutes?: number;
  servings?: number;
  ingredients: string;
  method: string;
  suggestions?: string;
  remarks?: string;
  source?: string;
}

export interface Recipe extends RecipeAttributes {
  id: number;
}
