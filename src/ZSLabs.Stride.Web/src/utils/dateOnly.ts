export function toDateInput(value: string | null | undefined) {
  if (!value) {
    return ''
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const offset = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offset).toISOString().slice(0, 10)
}

export function fromDateInput(value: string) {
  return value.length === 0 ? null : new Date(`${value}T00:00:00`).toISOString()
}