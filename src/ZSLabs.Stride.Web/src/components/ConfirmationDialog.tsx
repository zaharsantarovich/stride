import { useEffect, useId, useRef } from 'react'
import { createPortal } from 'react-dom'

interface ConfirmationDialogProps {
  title: string
  message: string
  confirmLabel: string
  isConfirming?: boolean
  onCancel: () => void
  onConfirm: () => void
}

export function ConfirmationDialog({ title, message, confirmLabel, isConfirming = false, onCancel, onConfirm }: ConfirmationDialogProps) {
  const titleId = useId()
  const descriptionId = useId()
  const cancelButtonRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    cancelButtonRef.current?.focus()

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape' && !isConfirming) {
        onCancel()
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => window.removeEventListener('keydown', handleKeyDown)
  }, [isConfirming, onCancel])

  return createPortal(
    <div
      className="fixed inset-0 z-[60] grid place-items-center bg-stride-ink/45 p-5"
      role="presentation"
      onMouseDown={() => { if (!isConfirming) onCancel() }}
    >
      <div
        aria-describedby={descriptionId}
        aria-labelledby={titleId}
        aria-modal="true"
        className="w-full max-w-md rounded-2xl border border-stride-border bg-stride-surface p-6 shadow-board"
        role="alertdialog"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <p className="text-xs font-semibold uppercase tracking-[0.22em] text-stride-danger">Confirm deletion</p>
        <h2 id={titleId} className="mt-2 font-display text-2xl text-stride-ink">{title}</h2>
        <p id={descriptionId} className="mt-3 text-sm leading-6 text-stride-muted">{message}</p>
        <div className="mt-6 flex justify-end gap-2">
          <button
            ref={cancelButtonRef}
            className="min-h-11 rounded-xl border border-stride-border bg-white px-4 py-2 text-sm font-semibold text-stride-ink focus:outline-none focus:ring-2 focus:ring-stride-accent disabled:opacity-60"
            disabled={isConfirming}
            type="button"
            onClick={onCancel}
          >
            Cancel
          </button>
          <button
            className="min-h-11 rounded-xl bg-stride-danger px-4 py-2 text-sm font-semibold text-white focus:outline-none focus:ring-2 focus:ring-stride-danger disabled:opacity-60"
            disabled={isConfirming}
            type="button"
            onClick={onConfirm}
          >
            {isConfirming ? 'Deleting...' : confirmLabel}
          </button>
        </div>
      </div>
    </div>,
    document.body,
  )
}