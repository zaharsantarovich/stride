import { useState } from 'react'
import type { FormEvent } from 'react'
import type { Comment } from '../api/contracts'
import { formatLocalDateTime } from '../utils/formatLocalDateTime'

interface CommentListProps {
  comments: Comment[]
  currentUserId?: number
  onAdd: (content: string) => Promise<void>
  onUpdate: (commentId: number, content: string) => Promise<void>
  onDelete: (commentId: number) => Promise<void>
}

export function CommentList({ comments, currentUserId, onAdd, onUpdate, onDelete }: CommentListProps) {
  const [draft, setDraft] = useState('')
  const [editingCommentId, setEditingCommentId] = useState<number | null>(null)
  const [editContent, setEditContent] = useState('')

  async function handleAdd(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (draft.trim().length === 0) {
      return
    }

    await onAdd(draft.trim())
    setDraft('')
  }

  async function handleUpdate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (editingCommentId === null || editContent.trim().length === 0) {
      return
    }

    await onUpdate(editingCommentId, editContent.trim())
    setEditingCommentId(null)
    setEditContent('')
  }

  return (
    <section className="rounded-2xl border border-stride-border/70 bg-white/80 p-4">
      <div className="flex items-center justify-between gap-3">
        <h4 className="font-semibold text-stride-ink">Comments</h4>
        <span className="text-xs uppercase tracking-[0.2em] text-stride-muted">{comments.length}</span>
      </div>
      <div className="mt-3 space-y-3">
        {comments.map((comment) => {
          const isAuthor = currentUserId === comment.authorId

          return (
            <article key={comment.id} className="rounded-xl border border-stride-border bg-stride-surface px-3 py-3">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm leading-6 text-stride-ink">{comment.content}</p>
                  <p className="mt-1 text-xs text-stride-muted">{formatLocalDateTime(comment.updatedAt ?? comment.createdAt)}</p>
                </div>
                {isAuthor ? (
                  <div className="flex gap-2 text-xs">
                    <button
                      className="font-semibold text-stride-accent"
                      onClick={() => {
                        setEditingCommentId(comment.id)
                        setEditContent(comment.content)
                      }}
                      type="button"
                    >
                      Edit
                    </button>
                    <button className="font-semibold text-stride-danger" onClick={() => void onDelete(comment.id)} type="button">
                      Delete
                    </button>
                  </div>
                ) : null}
              </div>
              {editingCommentId === comment.id ? (
                <form className="mt-3 grid gap-2" onSubmit={handleUpdate}>
                  <textarea
                    className="min-h-20 rounded-xl border border-stride-border bg-white px-3 py-2"
                    value={editContent}
                    onChange={(event) => setEditContent(event.target.value)}
                  />
                  <div className="flex gap-2">
                    <button className="rounded-lg bg-stride-accent px-3 py-2 text-xs font-semibold text-white" type="submit">
                      Save
                    </button>
                    <button
                      className="rounded-lg border border-stride-border px-3 py-2 text-xs font-semibold text-stride-ink"
                      onClick={() => setEditingCommentId(null)}
                      type="button"
                    >
                      Cancel
                    </button>
                  </div>
                </form>
              ) : null}
            </article>
          )
        })}
        {comments.length === 0 ? <p className="text-sm text-stride-muted">No comments yet.</p> : null}
      </div>
      <form className="mt-4 grid gap-2" onSubmit={handleAdd}>
        <textarea
          className="min-h-20 rounded-xl border border-stride-border bg-white px-3 py-2"
          placeholder="Add a comment"
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
        />
        <button className="justify-self-start rounded-lg bg-stride-accent px-3 py-2 text-xs font-semibold text-white" type="submit">
          Post comment
        </button>
      </form>
    </section>
  )
}