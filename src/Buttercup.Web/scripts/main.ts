import PopoverMenu from './popover-menu';
import recipesFilter from './recipes-filter';

(() => {
  document.body.classList.add('js-enabled');

  const topBarMenuButton = document.getElementById('top-bar__menu-button');

  if (topBarMenuButton) {
    new PopoverMenu(
      document,
      topBarMenuButton,
      document.getElementById('top-bar__menu-popover')!,
      { placement: 'bottom-end' }
    );
  }

  const recipeFilterInput = document.getElementById(
    'recipe-index__filter'
  ) as HTMLInputElement;

  if (recipeFilterInput) {
    recipesFilter(
      recipeFilterInput,
      document.getElementById('recipes-index__table') as HTMLTableElement
    );
  }
})();
