import assert from "node:assert/strict";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import test from "node:test";

import {
  PACKAGE_IDS,
  createChecksums,
  evaluateReleaseState,
  parsePackageVersion,
  resolveMetadata,
  selectPreviousTag,
  validateInputs,
} from "./release-tools.mjs";

test("validates stable, nightly, beta, and RC inputs", () => {
  assert.doesNotThrow(() => validateInputs({ prerelease: false, confirmFullRelease: true, suffix: "" }));
  for (const suffix of ["nightly-001", "beta.2", "rc-1"])
    assert.doesNotThrow(() => validateInputs({ prerelease: true, confirmFullRelease: false, suffix }));
  assert.throws(() => validateInputs({ prerelease: true, confirmFullRelease: false, suffix: "" }), /suffix is required/);
  assert.throws(() => validateInputs({ prerelease: false, confirmFullRelease: true, suffix: "nightly-001" }), /cannot have/);
  assert.throws(() => validateInputs({ prerelease: false, confirmFullRelease: false, suffix: "" }), /confirm_full_release/);
});

test("extracts package versions without confusing package IDs", () => {
  assert.equal(parsePackageVersion("DisCatSharp.Extensions.TwoFactorCommands.3.3.4-nightly-001.nupkg", PACKAGE_IDS[2]), "3.3.4-nightly-001");
  assert.equal(parsePackageVersion("DisCatSharp.Extensions.OAuth2Web.3.3.4.nupkg", PACKAGE_IDS[2]), null);
});

test("requires every extension package to have the same built version", () => {
  withTempDirectory((directory) => {
    for (const packageId of PACKAGE_IDS)
      writeFileSync(join(directory, `${packageId}.3.3.4-nightly-001.nupkg`), packageId);
    assert.deepEqual(resolveMetadata({ artifactDirectory: directory, prerelease: true, suffix: "nightly-001" }), {
      version: "3.3.4-nightly-001",
      tag: "v3.3.4-nightly-001",
      title: "DisCatSharp Extensions v3.3.4-nightly-001",
    });
  });

  withTempDirectory((directory) => {
    PACKAGE_IDS.forEach((packageId, index) => writeFileSync(join(directory, `${packageId}.${index === 2 ? "3.3.5" : "3.3.4"}.nupkg`), packageId));
    assert.throws(() => resolveMetadata({ artifactDirectory: directory, prerelease: false, suffix: "" }), /versions disagree/);
  });
});

test("stable notes ignore nightlies while prerelease notes use the latest release", () => {
  const releases = [
    release("v3.3.4-nightly-001", true, "2026-08-11"),
    release("v3.3.3", false, "2026-04-20"),
    release("alpha-005", true, "2023-03-29"),
  ];
  assert.equal(selectPreviousTag({ releases, prerelease: true, currentTag: "v3.3.4-nightly-002" }), "v3.3.4-nightly-001");
  assert.equal(selectPreviousTag({ releases, prerelease: false, currentTag: "v3.3.4" }), "v3.3.3");
});

test("draft state is resumable and published state is verification-only", () => {
  const expected = { tag: "v3.3.4-nightly-001", targetCommit: "abc123" };
  assert.equal(evaluateReleaseState(null, expected), "create");
  assert.equal(evaluateReleaseState({ tag_name: expected.tag, target_commitish: expected.targetCommit, draft: true }, expected), "resume");
  assert.equal(evaluateReleaseState({ tag_name: expected.tag, target_commitish: expected.targetCommit, draft: false }, expected), "verify");
  assert.throws(() => evaluateReleaseState({ tag_name: expected.tag, target_commitish: "different", draft: true }, expected), /targets different/);
});

test("writes deterministic checksums for all release assets", () => {
  withTempDirectory((directory) => {
    writeFileSync(join(directory, "b.nupkg"), "b");
    writeFileSync(join(directory, "a.snupkg"), "a");
    writeFileSync(join(directory, "README.md"), "readme");
    createChecksums(directory);
    const checksums = readFileSync(join(directory, "SHA256SUMS"), "utf8").trim().split("\n");
    assert.match(checksums[0], /  a\.snupkg$/);
    assert.match(checksums[1], /  b\.nupkg$/);
    assert.match(checksums[2], /  README\.md$/);
  });
});

function release(tagName, isPrerelease, publishedAt) {
  return { tagName, isPrerelease, isDraft: false, publishedAt };
}

function withTempDirectory(operation) {
  const directory = mkdtempSync(join(tmpdir(), "dcs-ext-release-test-"));
  try {
    operation(directory);
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
}
