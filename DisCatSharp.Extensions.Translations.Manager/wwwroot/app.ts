(() => {
	type Locale = string;
	type TranslationMap = Record<Locale, string>;
	type StringsMap = Record<string, TranslationMap>;

	type AuditUsage = {
		file: string;
		line: number;
		reason?: string;
		expression?: string;
	};

	type AuditReport = {
		usedKeysCount: number;
		definedKeysCount: number;
		missingKeys: string[];
		unusedKeys: string[];
		dynamicKeyUsages: AuditUsage[];
		dynamicKeyCandidates?: Record<string, string[]>;
		keyUsages: Record<string, AuditUsage[]>;
		repoRoot: string;
	};

	type State = {
		report: AuditReport | null;
		strings: StringsMap;
		locales: Locale[];
		selected: string | null;
		realKeys: Set<string>;
	};

	const PRIMARY_LOCALE = "en-US";
	const FALLBACK_LOCALES: Locale[] = [
		"da",
		"de",
		"en-GB",
		"en-US",
		"es-ES",
		"fr",
		"hr",
		"it",
		"lt",
		"hu",
		"nl",
		"no",
		"pl",
		"pt-BR",
		"ro",
		"fi",
		"sv-SE",
		"vi",
		"tr",
		"cs",
		"el",
		"bg",
		"ru",
		"uk",
		"hi",
		"th",
		"zh-CN",
		"ja",
		"zh-TW",
		"ko",
	];

	const storedScheme = localStorage.getItem("vscodeScheme");
	let vscodeScheme =
		storedScheme === "vscode-insiders" || storedScheme === "vscode"
			? storedScheme
			: navigator.userAgent.includes("Insider")
			? "vscode-insiders"
			: "vscode";
	const REPO_BLOB_BASE =
		"https://github.com/Aiko-IT-Systems/ScWikeloGrind/blob/main";

	const state: State = {
		report: null,
		strings: {},
		locales: [],
		selected: null,
		realKeys: new Set<string>(),
	};

	const el = (id: string) => document.getElementById(id)!;
	const keyList = el("keyList");
	const details = el("detailsBody");
	const summary = el("summary");
	const scheme = el("scheme") as HTMLSelectElement;
	const search = el("search") as HTMLInputElement;
	const filter = el("filter") as HTMLSelectElement;
	const downloadBtn = el("downloadStrings");
	const modalHost = el("modalHost");

	el("refresh").addEventListener("click", refreshAll);
	el("addKey").addEventListener("click", onAddKey);
	el("beautify").addEventListener("click", beautify);
	downloadBtn.addEventListener("click", downloadStrings);
	scheme.addEventListener("change", () => {
		vscodeScheme = scheme.value;
		localStorage.setItem("vscodeScheme", vscodeScheme);
		renderDetails(state.selected);
	});

	scheme.value = vscodeScheme;
	search.addEventListener("input", renderKeyList);
	filter.addEventListener("change", renderKeyList);

	async function loadAll() {
		const [report, strings, locales] = await Promise.all([
			fetchJson<AuditReport>("/api/report"),
			fetchJson<StringsMap>("/api/strings"),
			fetchJson<Locale[]>("/api/locales"),
		]);

		state.report = report;
		state.realKeys = new Set(Object.keys(strings));
		state.strings = { ...strings };

		const missingKeys = report?.missingKeys ?? [];
		missingKeys.forEach((key) => {
			if (!Object.prototype.hasOwnProperty.call(state.strings, key)) {
				state.strings[key] = { [PRIMARY_LOCALE]: "" };
			}
		});

		state.locales =
			locales && locales.length > 0 ? locales : FALLBACK_LOCALES;
		summary.textContent = `Used ${report.usedKeysCount} / Defined ${report.definedKeysCount} | Missing ${report.missingKeys.length} | Unused ${report.unusedKeys.length} | Dynamic ${report.dynamicKeyUsages.length}`;
		renderKeyList();
		renderDetails(state.selected);
	}

	function renderKeyList() {
		if (!state.report) return;
		const query = search.value.toLowerCase();
		const filterValue = filter.value;
		const expectedLocales = collectExpectedLocales(state.strings);

		const translationKeys = Object.keys(state.strings);
		const missingKeys = state.report?.missingKeys ?? [];
		const dynamicKeys = (state.report.dynamicKeyUsages || [])
			.map((d) => keyFromDynamic(d))
			.filter((k) => k && k.includes("."));
		const keys = Array.from(
			new Set([...translationKeys, ...dynamicKeys, ...missingKeys])
		).sort((a, b) => a.localeCompare(b));

		keyList.innerHTML = "";
		const template = document.getElementById(
			"key-item-template"
		) as HTMLTemplateElement;

		keys.forEach((key) => {
			const translations = state.strings[key] || {};
			const valuesText = Object.values(translations)
				.join(" ")
				.toLowerCase();
			if (
				query &&
				!key.toLowerCase().includes(query) &&
				!valuesText.includes(query)
			)
				return;

			const isReportedMissing = missingKeys.includes(key);
			const isDynamic = state.report!.dynamicKeyUsages.some(
				(d) => keyFromDynamic(d) === key
			);
			const isMissingKey = !isDynamic && (!state.realKeys.has(key) || isReportedMissing);
			const isUnused = state.report!.unusedKeys.includes(key);
			const primaryMissing =
				!translations[PRIMARY_LOCALE] ||
				translations[PRIMARY_LOCALE].trim().length === 0;
			const isMissingLocale =
				!isMissingKey &&
				!isDynamic &&
				(primaryMissing || hasMissingLocale(translations, expectedLocales));
			const isDynamicBadge = isDynamic;

			if (filterValue === "kmissing" && !isMissingKey) return;
			if (filterValue === "lmissing" && !isMissingLocale) return;
			if (filterValue === "unused" && !isUnused) return;
			if (filterValue === "dynamic" && !isDynamicBadge) return;

			const node = template.content.firstElementChild!.cloneNode(
				true
			) as HTMLElement;
			node.querySelector<HTMLElement>(".key-text")!.textContent = key;
			const badges = node.querySelector<HTMLElement>(".badges")!;
			badges.style.flexWrap = "wrap";
			if (isMissingKey) badges.append(makeBadge("Missing Key", "danger"));
			if (isUnused) badges.append(makeBadge("Unused", "warn"));
			if (!isMissingKey && isMissingLocale)
				badges.append(makeBadge("Missing Locale", "danger"));
			if (isDynamicBadge) badges.append(makeBadge("Dynamic", "success"));
			node.addEventListener("click", () => renderDetails(key));
			if (state.selected === key) node.classList.add("selected");
			keyList.append(node);
		});
	}

	function collectExpectedLocales(strings: StringsMap) {
		const set = new Set<Locale>();
		set.add(PRIMARY_LOCALE);
		Object.values(strings).forEach((map) => {
			Object.keys(map || {}).forEach((loc) => set.add(loc));
		});
		return set;
	}

	function hasMissingLocale(translations: TranslationMap, expected: Set<Locale>) {
		for (const loc of expected) {
			const text = translations?.[loc] ?? "";
			if (!text || text.trim().length === 0) return true;
		}
		return false;
	}

	function renderDetails(key: string | null) {
		state.selected = key;
		if (!key) {
			details.className = "details-empty";
			details.textContent = "Select a key to edit";
			return;
		}

		const existsInStrings = state.realKeys.has(key);
		const isDynamicKey = state.report?.dynamicKeyUsages.some(
			(d) => keyFromDynamic(d) === key
		);
		const dynamicCandidates = state.report?.dynamicKeyCandidates?.[key] ?? [];
		const translations = state.strings[key] || { [PRIMARY_LOCALE]: "" };
		const locales = orderLocales(Object.keys(translations));

		const wrapper = document.createElement("div");
		wrapper.className = "details";
		const title = document.createElement("div");
		title.innerHTML = `<strong>${key}</strong>${
			existsInStrings || isDynamicKey
				? ""
				: ' <span class="badge warn">Missing Key</span>'
		}`;
		wrapper.append(title);
		if (!existsInStrings && !isDynamicKey) {
			const hint = document.createElement("div");
			hint.className = "details-empty";
			hint.textContent =
				"This key is missing from strings.json. Fill in values and Save to create it.";
			wrapper.append(hint);
		}

		if (isDynamicKey) {
			const dynNote = document.createElement("div");
			dynNote.className = "details-empty";
			dynNote.textContent = "Dynamic key: manage concrete keys instead.";
			wrapper.append(dynNote);
		} else {
			const transWrap = document.createElement("div");
			transWrap.className = "translations";
			locales.forEach((locale) => {
				transWrap.append(makeLocaleRow(locale, translations[locale] ?? ""));
			});

			const addLocaleRow = document.createElement("div");
			addLocaleRow.className = "translation-add-row";
			const addSelect = document.createElement("select");
			const availableLocales = orderLocales(state.locales);
			const existing = new Set(locales);
			const options = availableLocales.filter((l) => !existing.has(l));
			addSelect.innerHTML = `<option value="">Select locale</option>${options
				.map((l) => `<option value="${l}">${l}</option>`)
				.join("")}`;
			const addButton = document.createElement("button");
			addButton.textContent = "Add locale";
			addButton.className = "secondary";
			addButton.onclick = () => {
				const loc = addSelect.value;
				if (!loc) return;
				if (transWrap.querySelector(`[data-locale="${loc}"]`)) return;
				transWrap.insertBefore(makeLocaleRow(loc, ""), addLocaleRow);
				addSelect.value = "";
			};
			const actions = document.createElement("div");
			actions.className = "footer-actions";
			const saveBtn = document.createElement("button");
			saveBtn.textContent = "Save";
			saveBtn.classList.add("success");
			saveBtn.onclick = () => saveKey(key, transWrap);
			const deleteBtn = document.createElement("button");
			deleteBtn.textContent = "Delete";
			deleteBtn.className = "danger";
			deleteBtn.onclick = () => deleteKey(key);
			actions.append(saveBtn, deleteBtn);
			addLocaleRow.append(addSelect, addButton, actions);
			transWrap.append(addLocaleRow);
			wrapper.append(transWrap);
		}

		const usageSection = document.createElement("div");
		usageSection.className = "usage";
		usageSection.innerHTML = "<strong>Usages</strong>";
		const usageList = document.createElement("div");
		const usages = state.report?.keyUsages[key] || [];
		const dynUsages = (state.report?.dynamicKeyUsages || []).filter(
			(d) => keyFromDynamic(d) === key
		);
		const usageTemplate = document.getElementById(
			"usage-row-template"
		) as HTMLTemplateElement;
		const repoRoot = (state.report?.repoRoot || "").replace(/\\/g, "/");
		if (usages.length > 0) {
			usages.forEach((u) => {
				const row = usageTemplate.content.firstElementChild!.cloneNode(
					true
				) as HTMLElement;
				row.querySelector<HTMLElement>(
					".usage-file"
				)!.textContent = `${u.file} : ${u.line}`;
				const fullPath = `${repoRoot}/${u.file}`.replace(/\\/g, "/");
				const openBtn = ensureUsageOpen(row);
				openBtn.onclick = () => {
					const codeUrl = `${vscodeScheme}://file/${encodeURI(
						fullPath
					)}:${u.line}`;
					window.open(codeUrl, "_blank");
				};
				const gitBtn =
					row.querySelector<HTMLButtonElement>(".usage-copy")!;
				const gitUrl = `${REPO_BLOB_BASE}/${u.file.replace(
					/\\/g,
					"/"
				)}#L${u.line}`;
				gitBtn.onclick = () => copyText(gitUrl, "Git link copied");
				usageList.append(row);
			});
		}
		if (dynUsages.length > 0) {
			dynUsages.forEach((d) => {
				const row = usageTemplate.content.firstElementChild!.cloneNode(
					true
				) as HTMLElement;
				row.querySelector<HTMLElement>(".usage-file")!.textContent = `${
					d.file
				} : ${d.line} (dynamic: ${d.reason ?? ""})`;
				const fullPath = `${repoRoot}/${d.file}`.replace(/\\/g, "/");
				const openBtn = ensureUsageOpen(row);
				openBtn.onclick = () => {
					const codeUrl = `${vscodeScheme}://file/${encodeURI(
						fullPath
					)}:${d.line}`;
					window.open(codeUrl, "_blank");
				};
				const gitBtn =
					row.querySelector<HTMLButtonElement>(".usage-copy")!;
				const gitUrl = `${REPO_BLOB_BASE}/${d.file.replace(
					/\\/g,
					"/"
				)}#L${d.line}`;
				gitBtn.onclick = () => copyText(gitUrl, "Git link copied");
				usageList.append(row);
			});
		}
		if (usages.length === 0 && dynUsages.length === 0) {
			const none = document.createElement("div");
			none.textContent = "No usages found.";
			none.className = "details-empty";
			usageList.append(none);
		}
		usageSection.append(usageList);
		wrapper.append(usageSection);

		if (isDynamicKey && dynamicCandidates.length > 0) {
			const candidateSection = document.createElement("div");
			candidateSection.className = "usage";
			candidateSection.innerHTML = "<strong>Candidate keys</strong>";
			const candidateList = document.createElement("div");
			candidateList.className = "candidate-list";
			dynamicCandidates.forEach((cand) => {
				const row = document.createElement("div");
				row.className = "details-empty";
				row.textContent = cand;
				row.style.cursor = "pointer";
				row.onclick = () => renderDetails(cand);
				candidateList.append(row);
			});
			candidateSection.append(candidateList);
			wrapper.append(candidateSection);
		}

		details.className = "";
		details.innerHTML = "";
		details.append(wrapper);
	}

	function makeLocaleRow(locale: string, value: string) {
		const row = document.createElement("div");
		row.className = "translation-row";
		row.dataset.locale = locale;
		const input = document.createElement("input");
		input.value = locale;
		input.disabled = true;
		const textarea = document.createElement("textarea");
		textarea.value = value ?? "";
		autosizeText(textarea);
		textarea.addEventListener("input", () => autosizeText(textarea));
		const remove = document.createElement("button");
		remove.textContent = "Remove";
		remove.className = "danger";
		remove.onclick = () => {
			if (locale === PRIMARY_LOCALE) return;
			row.remove();
		};
		if (locale === PRIMARY_LOCALE) {
			remove.disabled = true;
			remove.textContent = "Primary";
		}
		row.append(input, textarea, remove);
		return row;
	}

	function autosizeText(textarea: HTMLTextAreaElement) {
		textarea.style.height = "auto";
		textarea.style.height = `${textarea.scrollHeight}px`;
	}

	function orderLocales(locales: string[]) {
		return [...locales].sort((a, b) => {
			if (a === b) return 0;
			if (a === PRIMARY_LOCALE) return -1;
			if (b === PRIMARY_LOCALE) return 1;
			return a.localeCompare(b);
		});
	}

	async function saveKey(key: string, transWrap: HTMLElement) {
		const ok = await confirmModal(
			"Save changes?",
			"This will write updates to strings.json."
		);
		if (!ok) return;
		const translations: TranslationMap = {};
		transWrap
			.querySelectorAll<HTMLElement>(".translation-row")
			.forEach((row) => {
				const loc = row.dataset.locale;
				const text = (
					row.querySelector("textarea") as HTMLTextAreaElement
				).value;
				if (loc) translations[loc] = text;
			});
		const res = await fetch(`/api/strings/${encodeURIComponent(key)}`, {
			method: "PUT",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({ key, translations }),
		});
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Save failed: ${msg}`);
			return;
		}
		await loadAll();
		renderDetails(key);
		showToast("Saved");
	}

	async function deleteKey(key: string) {
		const ok = await confirmModal(
			"Delete key?",
			`This removes '${key}' from strings.json.`
		);
		if (!ok) return;
		const res = await fetch(`/api/strings/${encodeURIComponent(key)}`, {
			method: "DELETE",
		});
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Delete failed: ${msg}`);
			return;
		}
		await loadAll();
		renderDetails(null);
		showToast("Deleted");
	}

	async function onAddKey() {
		const key = prompt("New translation key");
		if (!key) return;
		const res = await fetch("/api/strings", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify({
				key,
				translations: { [PRIMARY_LOCALE]: "" },
			}),
		});
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Add failed: ${msg}`);
			return;
		}
		await loadAll();
		renderDetails(key);
	}

	async function beautify() {
		const ok = await confirmModal(
			"Beautify strings.json?",
			"This will rewrite strings.json with formatting."
		);
		if (!ok) return;
		const res = await fetch("/api/strings/format", { method: "POST" });
		if (!res.ok) {
			const msg = await readError(res);
			alert(`Beautify failed: ${msg}`);
			return;
		}
		await loadAll();
		showToast("Formatted");
	}

	function makeBadge(text: string, tone: "warn" | "danger" | "success") {
		const span = document.createElement("span");
		span.className = `badge ${tone}`;
		span.textContent = text;
		return span;
	}

	function keyFromDynamic(dyn: AuditUsage | null | undefined) {
		if (!dyn) return "";
		const raw = dyn.expression || "";
		const normalized = normalizeDynamicKey(raw);
		return normalized || raw || "";
	}

	function normalizeDynamicKey(expr: string) {
		let s = expr.trim();
		if (s.startsWith("@$")) s = s.slice(2);
		else if (s.startsWith("$")) s = s.slice(1);
		s = s.replace(/^"/, "").replace(/"$/, "");
		const brace = s.indexOf("{");
		if (brace >= 0) s = s.slice(0, brace);
		return s.trim();
	}

	function ensureUsageOpen(row: HTMLElement) {
		let btn = row.querySelector<HTMLButtonElement>(".usage-open");
		if (!btn) {
			btn = document.createElement("button");
			btn.className = "usage-open";
			btn.textContent = "Open in VS Code";
			const actions =
				row.querySelector<HTMLElement>(".usage-actions") || row;
			actions.prepend(btn);
		}
		return btn;
	}

	async function refreshAll() {
		try {
			await loadAll();
			showToast("Refreshed");
		} catch (err: unknown) {
			const message = (err as Error)?.message || String(err);
			alert(`Refresh failed: ${message}`);
		}
	}

	async function downloadStrings() {
		try {
			const res = await fetch("/api/strings");
			if (!res.ok) throw new Error(await readError(res));
			const data = await res.json();
			const blob = new Blob([JSON.stringify(data, null, 2)], {
				type: "application/json",
			});
			const url = URL.createObjectURL(blob);
			const a = document.createElement("a");
			a.href = url;
			a.download = "strings.json";
			a.click();
			URL.revokeObjectURL(url);
			showToast("Downloaded strings.json");
		} catch (err: unknown) {
			alert(`Download failed: ${err}`);
		}
	}

	function copyText(text: string, toastMessage: string) {
		navigator.clipboard
			?.writeText(text)
			.then(() => showToast(toastMessage));
	}

	function showToast(message: string) {
		const toast = document.createElement("div");
		toast.className = "toast";
		toast.textContent = message;
		document.body.append(toast);
		setTimeout(() => toast.remove(), 2000);
	}

	function confirmModal(title: string, message: string) {
		return new Promise<boolean>((resolve) => {
			const backdrop = document.createElement("div");
			backdrop.className = "modal-backdrop";
			const modal = document.createElement("div");
			modal.className = "modal";
			modal.innerHTML = `<h3>${title}</h3><p>${message}</p>`;
			const actions = document.createElement("div");
			actions.className = "modal-actions";
			const cancel = document.createElement("button");
			cancel.className = "ghost";
			cancel.textContent = "Cancel";
			const ok = document.createElement("button");
			ok.className = "danger";
			ok.textContent = "Confirm";
			cancel.onclick = () => {
				backdrop.remove();
				resolve(false);
			};
			ok.onclick = () => {
				backdrop.remove();
				resolve(true);
			};
			actions.append(cancel, ok);
			modal.append(actions);
			backdrop.append(modal);
			modalHost.append(backdrop);
		});
	}

	async function fetchJson<T>(url: string) {
		const res = await fetch(url);
		if (!res.ok) {
			const msg = await readError(res);
			throw new Error(msg);
		}
		return res.json() as Promise<T>;
	}

	async function readError(res: Response) {
		try {
			const data = await res.json();
			if ((data as { message?: string })?.message)
				return (data as { message: string }).message;
			return `${res.status} ${res.statusText}`;
		} catch {
			return `${res.status} ${res.statusText}`;
		}
	}

	loadAll();
})();
