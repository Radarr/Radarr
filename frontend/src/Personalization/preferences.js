const ACCENT_COLOR_PATTERN = /^(#[0-9a-f]{3,8}|[a-z]+)$/i;

function normalizeAccentColor(value) {
  const color = String(value).trim();
  return ACCENT_COLOR_PATTERN.test(color) ? color : '#ffc230';
}

function moveDashboardWidget(widgets, id, offset) {
  const index = widgets.findIndex((widget) => widget.id === id);
  if (index === -1) {
    return widgets;
  }

  const target = Math.max(0, Math.min(widgets.length - 1, index + offset));
  if (index === target) {
    return widgets;
  }

  const next = [...widgets];
  const [item] = next.splice(index, 1);
  next.splice(target, 0, item);
  return next;
}

module.exports = { moveDashboardWidget, normalizeAccentColor };
