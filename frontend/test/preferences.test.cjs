const test = require('node:test');
const assert = require('node:assert/strict');
const { moveDashboardWidget, normalizeAccentColor } = require('../src/Personalization/preferences');

test('normalizes supported accent colors and rejects unsafe values', () => {
  assert.equal(normalizeAccentColor(' #12abEF '), '#12abEF');
  assert.equal(normalizeAccentColor('rebeccapurple'), 'rebeccapurple');
  assert.equal(normalizeAccentColor('url(javascript:alert(1))'), '#ffc230');
});

test('moves dashboard widgets without mutating persisted state', () => {
  const widgets = [{ id: 'one', isVisible: true }, { id: 'two', isVisible: true }];
  const moved = moveDashboardWidget(widgets, 'two', -1);
  assert.deepEqual(moved.map((widget) => widget.id), ['two', 'one']);
  assert.deepEqual(widgets.map((widget) => widget.id), ['one', 'two']);
});
