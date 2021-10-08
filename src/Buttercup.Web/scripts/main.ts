import PopoverMenu from './popover-menu';
import recipesFilter from './recipes-filter';

document.body.classList.add('js-enabled');

initializeTopBar();
initializeRecipeFilter();

function initializeTopBar() {
  const menuButton = document.getElementById('top-bar__menu-button');

  if (menuButton) {
    new PopoverMenu(
      document,
      menuButton,
      document.getElementById('top-bar__menu-popover')!,
      { placement: 'bottom-end' },
    );
  }
}

function initializeRecipeFilter() {
  const input = document.getElementById('recipe-index__filter') as HTMLInputElement;

  if (input) {
    recipesFilter(input, document.getElementById('recipes-index__table') as HTMLTableElement);
  }
}
