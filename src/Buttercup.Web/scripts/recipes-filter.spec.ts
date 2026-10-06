import recipesFilter from './recipes-filter';

describe('recipesFilter', () => {
  let fixture: HTMLElement;
  let filterInput: HTMLInputElement;
  let list: HTMLUListElement;
  let rows: {
    applePie: HTMLElement;
    chickenPie: HTMLElement;
    pizza: HTMLElement;
  };

  const addRow = (recipeTitle: string) => {
    const item = document.createElement('li');
    item.textContent = recipeTitle;

    list.appendChild(item);

    return item;
  };

  const initializeFilter = (initialFilter = '') => {
    filterInput.value = initialFilter;

    recipesFilter(filterInput, list);
  };

  const triggerFilterInput = (newFilter: string) => {
    filterInput.value = newFilter;
    filterInput.dispatchEvent(new Event('input'));
  };

  beforeEach(() => {
    document.body.appendChild((fixture = document.createElement('div')));
    fixture.appendChild((filterInput = document.createElement('input')));
    fixture.appendChild((list = document.createElement('ul')));

    rows = {
      applePie: addRow('Apple pie'),
      chickenPie: addRow('Chicken pie'),
      pizza: addRow('Ham and pineapple pizza'),
    };
  });

  afterEach(() => fixture.remove());

  it('shows and hides rows based on the initial filter', () => {
    initializeFilter('apple');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(
      false,
    );
    expect(rows.chickenPie.classList.contains('recipes-index--hidden')).toBe(
      true,
    );
    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(false);
  });

  it('shows and hides rows as the filter changes', () => {
    initializeFilter();

    triggerFilterInput('pie');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(
      false,
    );
    expect(rows.chickenPie.classList.contains('recipes-index--hidden')).toBe(
      false,
    );
    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(true);

    triggerFilterInput('');

    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(false);
  });

  it('matches words and partial words in any order', () => {
    initializeFilter('pizza   appl ha');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(
      true,
    );
    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(false);
  });

  it('ignores case', () => {
    initializeFilter('cHiCkEn');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(
      true,
    );
    expect(rows.chickenPie.classList.contains('recipes-index--hidden')).toBe(
      false,
    );
  });
});
