---
description: Завести git-репозиторий, если его нет
allowed-tools: Bash(git:*), Read
---

## 1. Проверь

    git rev-parse --is-inside-work-tree
    git config user.name
    git config user.email

Репозиторий уже есть — так и скажи, ничего не делай.
Не настроены имя и почта — скажи, что коммитить нечем:

    git config --global user.name "Имя"
    git config --global user.email "почта"

## 2. Заведи

    git init -q
    git symbolic-ref HEAD refs/heads/main
    git add -A
    git commit -m "initial"

Перед первым коммитом убедись, что `.gitignore` на месте и в нём есть
`Library/`, `Temp/`, `obj/`, `Logs/`, `Build/`, `UserSettings/`. Их нет —
скажи, что нужен `install.js`, и не коммить: гигабайты `Library/` в истории
удаляются потом только переписыванием истории.

Проверь, что `*.meta` НЕ игнорируются: `git check-ignore -v Assets/**/*.meta`
не должен ничего вернуть. В `.meta` живут GUID — потеряешь их, отвалятся
все ссылки в сценах и префабах.

## 3. GitHub — руками, и порядок важен

Скажи пользователю, не делай сам:

1. Создать на GitHub **ПУСТОЙ** репозиторий: без README, без .gitignore,
   без лицензии. Иначе первый пуш упрётся в расхождение историй.
2. `git remote add origin <ссылка>`
3. `git push -u origin main`

`.claude/` и `CLAUDE.md` едут в репозиторий вместе с проектом — история
решений должна храниться с кодом.
