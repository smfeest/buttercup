import { Client, fetchExchange, gql } from '@urql/core';
import { authExchange } from '@urql/exchange-auth';

/* eslint-disable @typescript-eslint/no-unsafe-assignment */

const AUTHENTICATE_QUERY = gql`
  mutation ($input: AuthenticateInput!) {
    authenticate(input: $input) {
      accessToken
    }
  }
`;

const CREATE_COMMENT_QUERY = gql`
  mutation CreateComment(
    $recipeId: Long!
    $attributes: CommentAttributesInput!
  ) {
    createComment(input: { recipeId: $recipeId, attributes: $attributes }) {
      comment {
        id
      }
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

const HARD_DELETE_RECIPE_QUERY = gql`
  mutation HardDeleteRecipe($id: Long!) {
    hardDeleteRecipe(input: { id: $id }) {
      deleted
    }
  }
`;

/* eslint-enable */

const DEFAULT_COMMENT_ATTRIBUTES: CommentAttributes = {
  body: 'Everything tastes better with a spoonful of marmite.',
};

const DEFAULT_RECIPE_ATTRIBUTES: RecipeAttributes = {
  title: 'Italian white bean soup',
  servings: 2,
  ingredients: [
    '400g dried haricot or flageolet beans',
    'Salt',
    'Butter',
    '1 carrot',
    '1 onion, halved',
    '2 leeks, in chunks',
    '1 stalk celery',
    'Chicken stock',
    '3 egg yolks',
    '200ml cream',
    'Peas, boiled',
    'Croûtons',
  ].join('\n'),
  method: [
    'Soak beans in water for 12 hours, then drain them.',
    'Add the drained beans to a stock pot with a little salt, butter, carrot, onion, two leeks, and a stick of celery.',
    'Cover with water, and simmer until the vegetables are well cooked.',
    'Drain the beans and vegetables, discarding the vegetables.',
    'Purée the beans, adding stock as necessary to get a smooth consistency.',
    'Transfer purée to a pot, and add stock to get the desired consistency. Bring to a boil, and keep hot until serving.',
    'Combine the egg yolks with the cream, and add this to the soup.',
    'Transfer the soup to a warm dish, add some boiled green peas, and serve with fried croûtons handed separately.',
  ].join('\n'),
  source:
    "The cook's Decameron : a study in taste, containing over two hundred recipes for Italian dishes",
};

export const api = (baseUrl: string, username: string) => {
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
                email: `${username}@example.com`,
                password: `${username}-pass`,
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
    async createComment(
      recipeId: number,
      explicitAttributes: Partial<CommentAttributes> = {},
    ): Promise<Comment> {
      const attributes = {
        ...DEFAULT_COMMENT_ATTRIBUTES,
        ...explicitAttributes,
      };

      const result = await client.mutation<{
        createComment: { comment: { id: number } };
      }>(CREATE_COMMENT_QUERY, { recipeId, attributes });

      if (!result.data) {
        throw new Error('Failed to insert comment');
      }

      const id = result.data.createComment.comment.id;
      return { id, ...attributes };
    },
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
    async hardDeleteRecipe(id: number) {
      await client.mutation(HARD_DELETE_RECIPE_QUERY, { id });
    },
  };
};

export interface CommentAttributes {
  body: string;
}

export interface Comment extends CommentAttributes {
  id: number;
}

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
