import recipesFilter from './recipes-filter';

describe('recipesFilter', () => {
  let fixture: HTMLElement;
  let filterInput: HTMLInputElement;
  let table: HTMLTableElement;
  let tableBody: HTMLTableSectionElement;
  let rows: {
    applePie: HTMLElement,
    chickenPie: HTMLElement,
    pizza: HTMLElement,
  };

  beforeEach(() => {
    document.body.appendChild(fixture = document.createElement('div'));
    fixture.appendChild(filterInput = document.createElement('input'));
    fixture.appendChild(table = document.createElement('table'));
    table.appendChild(tableBody = document.createElement('tbody'));

    rows = {
      applePie: addRow('Apple pie'),
      chickenPie: addRow('Chicken pie'),
      pizza: addRow('Ham and pineapple pizza'),
    };
  });

  afterEach(() => fixture.remove());

  function addRow(recipeTitle: string) {
    const cell = document.createElement('td');
    cell.textContent = recipeTitle;

    const row = document.createElement('tr');
    row.appendChild(cell);

    tableBody.appendChild(row);

    return row;
  }

  function initializeFilter(initialFilter = '') {
    filterInput.value = initialFilter;

    recipesFilter(filterInput, table);
  }

  function triggerFilterInput(newFilter: string) {
    filterInput.value = newFilter;
    filterInput.dispatchEvent(new Event('input'));
  }

  it('shows and hides rows based on the initial filter', () => {
    initializeFilter('apple');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(false);
    expect(rows.chickenPie.classList.contains('recipes-index--hidden')).toBe(true);
    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(false);
  });

  it('shows and hides rows as the filter changes', () => {
    initializeFilter();

    triggerFilterInput('pie');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(false);
    expect(rows.chickenPie.classList.contains('recipes-index--hidden')).toBe(false);
    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(true);

    triggerFilterInput('');

    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(false);
  });

  it('matches words and partial words in any order', () => {
    initializeFilter('pizza   appl ha');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(true);
    expect(rows.pizza.classList.contains('recipes-index--hidden')).toBe(false);
  });

  it('ignores case', () => {
    initializeFilter('cHiCkEn');

    expect(rows.applePie.classList.contains('recipes-index--hidden')).toBe(true);
    expect(rows.chickenPie.classList.contains('recipes-index--hidden')).toBe(false);
  });
});
